// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { defineComponent, onMounted, ref, watch } from "vue";
import Alert from "@Obsidian/Controls/alert.vue";
import DropDownList from "@Obsidian/Controls/dropDownList";
import TextBox from "@Obsidian/Controls/textBox";
import { dispatchBlockEvent, getSecurityGrant, provideSecurityGrant, useBlockGuid, useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { debounce } from "@Obsidian/Utility/util";
import { SearchOrder } from "@Obsidian/Enums/Blocks/Cms/ContentCollectionView/searchOrder";
import { ContentCollectionInitializationBox } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionView/contentCollectionInitializationBox";
import { SearchResultBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionView/searchResultBag";
import { SearchQueryBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionView/searchQueryBag";
import FiltersContainer from "./ContentCollectionView/filtersContainer.partial";
import { Guid } from "@Obsidian/Types";
import { toNumber } from "@Obsidian/Utility/numberUtils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { emptyGuid } from "@Obsidian/Utility/guid";
import { SortOrdersKey } from "./ContentCollectionView/types";


/**
 * Parses the window query string into the initial filter values that will be used.
 *
 * @param filterNames The names of the filters that are known by this block instance.
 *
 * @returns An object that contains the query string values that match a filter.
 */
function getQueryStringFilterValues(filterNames: string[]): Record<string, string> {
    const params: Record<string, string> = {};

    for (const entry of new URLSearchParams(window.location.search).entries()) {
        if (filterNames.some(n => n.toLowerCase() === entry[0].toLowerCase()) && entry[1] !== "") {
            params[entry[0].toLowerCase()] = entry[1];
        }
    }

    return params;
}

/**
 * Updates the window query string to match the search parameters.
 *
 * @param query The full-text query that was performed.
 * @param sortOrder The current sorting order applied to the results.
 * @param filterValues The values of the various filters.
 * @param filterNames The names of all known filters, this is used to include any additional query string parameters that we don't understand.
 */
function updateUrl(query: string, sortOrder: string, filterValues: Record<string, string>, filterNames: string[]): void {
    const qs: string[][] = [];

    // Add in the query text.
    if (query) {
        qs.push(["q", query]);
    }

    // Add in the sort order if it isn't default.
    if (sortOrder != SearchOrder.Relevance.toString()) {
        qs.push(["s", sortOrder.toString()]);
    }

    // Add in all the filter values if they are not blank.
    for (const key in filterValues) {
        if (filterValues[key]) {
            qs.push([key, filterValues[key]]);
        }
    }

    // Add in any query string parameters not related to us.
    for (const entry of new URLSearchParams(window.location.search).entries()) {
        const entryName = entry[0].toLowerCase();

        // Skip our special query string parameters.
        if (entryName === "q" || entryName === "s") {
            continue;
        }

        if (!filterNames.some(n => n.toLowerCase() === entryName)) {
            qs.push([entry[0].toLowerCase(), entry[1]]);
        }
    }

    // Update the URL in the window.
    if (qs.length > 0) {
        const querystring = qs.map(q => `${q[0]}=${q[1]}`).join("&");
        window.history.replaceState(null, "", `${window.location.pathname}?${querystring}`);
    }
    else {
        window.history.replaceState(null, "", window.location.pathname);
    }
}

/**
 * Updates the results element with additional data from the search result bag.
 *
 * @param resultsContainerElement The element that will contain all results.
 * @param results The results that were returned by the server.
 * @param seeMore The callback to use when the js-more element is clicked.
 */
function updateResults(resultsContainerElement: HTMLElement, results: SearchResultBag, seeMore: ((sourceGuid: Guid) => void)): void {
    for (const resultSource of results.resultSources ?? []) {
        const sourceGuid = resultSource.sourceGuid;

        if (!sourceGuid) {
            continue;
        }

        let sourceContainerElement = resultsContainerElement.querySelector<HTMLDivElement>(`.result-source-${sourceGuid.toLowerCase()}`);

        if (!sourceContainerElement) {
            sourceContainerElement = document.createElement("div");
            sourceContainerElement.classList.add("results", `result-source-${sourceGuid.toLowerCase()}`);
            sourceContainerElement.innerHTML = resultSource.template ?? "";

            const newSeeMoreElement = sourceContainerElement.querySelector(".js-more");
            if (newSeeMoreElement) {
                newSeeMoreElement.addEventListener("click", (e) => {
                    e.preventDefault();
                    seeMore(sourceGuid);
                });
            }

            resultsContainerElement.append(sourceContainerElement);
        }

        const resultItemsElement = sourceContainerElement.querySelector(".result-items");

        if (!resultItemsElement) {
            continue;
        }

        // Add each item from the results to the result container.
        for (const item of resultSource.results ?? []) {
            const itemElement = document.createElement("div");
            itemElement.innerHTML = item;

            for (const innerElement of Array.from(itemElement.children)) {
                innerElement.remove();
                resultItemsElement.append(innerElement);
            }

            const resultItemCount = toNumber(sourceContainerElement.dataset["resultItemCount"]) + 1;
            sourceContainerElement.dataset["resultItemCount"] = resultItemCount.toString();
        }

        // If there are no more results to load then hide the see more element.
        const seeMoreElement = sourceContainerElement.querySelector(".js-more");
        if (seeMoreElement && !resultSource.hasMore) {
            seeMoreElement.classList.add("hidden");
        }

        // If there are no results then add a "no-results" class to the source.
        if (!toNumber(sourceContainerElement.dataset["resultItemCount"])) {
            sourceContainerElement.classList.add("no-results");
        }
    }
}

/**
 * Gets the sort order items that can be selected by the person.
 *
 * @param allowed The allowed item names.
 *
 * @returns A list of items that can be selected.
 */
function getSortOrderItems(allowed: string[], trendingTerm: string): ListItemBag[] {
    /** The sort order items that can be selected by the person. */
    const sortOrderItems: ListItemBag[] = [];

    if (allowed.includes(SortOrdersKey.Relevance)) {
        sortOrderItems.push({
            value: SearchOrder.Relevance.toString(),
            text: "Relevance"
        });
    }

    if (allowed.includes(SortOrdersKey.Newest)) {
        sortOrderItems.push({
            value: SearchOrder.Newest.toString(),
            text: "Newest"
        });
    }

    if (allowed.includes(SortOrdersKey.Oldest)) {
        sortOrderItems.push({
            value: SearchOrder.Oldest.toString(),
            text: "Oldest"
        });
    }

    if (allowed.includes(SortOrdersKey.Trending)) {
        sortOrderItems.push({
            value: SearchOrder.Trending.toString(),
            text: trendingTerm
        });
    }

    if (allowed.includes(SortOrdersKey.Alphabetical)) {
        sortOrderItems.push({
            value: SearchOrder.Alphabetical.toString(),
            text: "Alphabetical"
        });
    }

    return sortOrderItems;
}

export default defineComponent({
    name: "Cms.ContentCollectionView",

    components: {
        Alert,
        DropDownList,
        FiltersContainer,
        TextBox
    },

    setup() {
        const config = useConfigurationValues<ContentCollectionInitializationBox>();
        const invokeBlockAction = useInvokeBlockAction();
        const blockGuid = useBlockGuid() ?? emptyGuid;
        const securityGrant = getSecurityGrant(config.securityGrantToken);

        // #region Values

        const urlSearchParams = new URLSearchParams(window.location.search);

        const blockError = ref(config.errorMessage);
        const filters = config.filters ?? [];
        const searchContainerElement = ref<HTMLElement | null>(null);
        const searchResultContainerElement = ref<HTMLElement | null>(null);
        const query = ref(urlSearchParams.get("q") || urlSearchParams.get("Q") || "");
        const filterValues = ref<Record<string, string>>(getQueryStringFilterValues(filters.map(f => f.label ?? "")));
        const sortOrder = ref(urlSearchParams.get("s") || urlSearchParams.get("S") || SearchOrder.Relevance.toString());
        const sortOrderItems = getSortOrderItems(config.enabledSortOrders ?? [], config.trendingTerm ?? "Trending");
        const totalResultsCount = ref(config.initialResults?.totalResultCount ?? 0);

        // #endregion

        // #region Computed Values

        // #endregion

        // #region Functions

        /**
         * Call the block action to perform the search and then update
         * the DOM with the results of the search.
         *
         * @param sourceGuid The unique identifier of the source we are loading more results for.
         * @param offset The offset into the source's results to load from.
         */
        const performSearch = async (sourceGuid?: Guid, offset?: number): Promise<void> => {
            // Update the URL to match this search.
            updateUrl(query.value, sortOrder.value, filterValues.value, filters.map(f => f.label ?? ""));

            const queryBag: SearchQueryBag = {
                text: query.value,
                filters: filterValues.value,
                sourceGuid: sourceGuid,
                offset: offset,
                order: toNumber(sortOrder.value)
            };

            const data = {
                query: queryBag
            };

            const result = await invokeBlockAction<SearchResultBag>("Search", data);

            if (result.isSuccess && result.data != null) {
                totalResultsCount.value = result.data.totalResultCount;
                processResults(result.data, !offset, sourceGuid);
            }
            else {
                console.error(result.errorMessage || "Unable to complete the search request.");
            }
        };

        /**
         * Process the results received from the search request by updating
         * the DOM with the additional results.
         *
         * @param data The data that contains the search results.
         * @param initialResults True if this call is for initial results being loaded.
         * @param sourceGuid The unique identifier of the source we are loading additional results for.
         */
        const processResults = (data: SearchResultBag, initialResults: boolean, sourceGuid?: Guid): void => {
            if (!searchResultContainerElement.value) {
                return;
            }

            if (initialResults) {
                searchResultContainerElement.value.innerHTML = "";
            }

            updateResults(searchResultContainerElement.value, data, onLoadMore);

            if (initialResults) {
                dispatchBlockEvent("content-collection-view-full-search", blockGuid);
            }
            else if (sourceGuid) {
                dispatchBlockEvent("content-collection-view-results-updated", blockGuid, {
                    sourceGuid
                });
            }
        };

        // #endregion

        // #region Event Handlers

        /**
         * Called when a "see more" button has been clicked and the person
         * wants to load more results for the source.
         *
         * @param sourceGuid
         */
        const onLoadMore = async (sourceGuid: Guid): Promise<void> => {
            if (!searchResultContainerElement.value) {
                return;
            }

            const sourceContainerElement = searchResultContainerElement.value.querySelector<HTMLDivElement>(`.result-source-${sourceGuid.toLowerCase()}`);
            if (!sourceContainerElement) {
                return;
            }

            const itemCount = toNumber(sourceContainerElement.dataset["resultItemCount"]);
            await performSearch(sourceGuid, itemCount);
        };

        /**
         * Event handler for when the individual clicks the clear button in
         * the query text box.
         */
        function onClearClick(): void {
            query.value = "";

            const inputElement = searchContainerElement.value?.querySelector("input");

            if (inputElement) {
                inputElement.focus();
            }
        }

        // #endregion

        provideSecurityGrant(securityGrant);

        const debounceSearch = debounce(performSearch, 450);

        // Any time the query text changes, perform a new search on a 450ms
        // delayed debounce timer.
        watch(query, () => {
            debounceSearch();
        });

        // If any of the other filter values change, perform a new search immediately.
        watch([filterValues, sortOrder], () => {
            performSearch();
        });

        onMounted(() => {
            const inputElement = searchContainerElement.value?.querySelector("input");

            if (inputElement) {
                inputElement.focus();
            }

            if (searchResultContainerElement.value) {
                if (config.initialResults) {
                    processResults(config.initialResults, true);
                }
                else if (config.preSearchContent) {
                    searchResultContainerElement.value.innerHTML = config.preSearchContent;
                }
            }
        });

        return {
            blockError,
            filters,
            filterValues,
            query,
            onClearClick,
            searchContainerElement,
            searchResultContainerElement,
            showSort: config.showSort,
            showFiltersPanel: config.showFiltersPanel,
            showFullTextSearch: config.showFullTextSearch,
            sortOrder,
            sortOrderItems,
            totalResultsCount
        };
    },

    template: `
<Alert v-if="blockError" alertType="warning" v-text="blockError" />

<div v-if="!blockError" class="collectionsearch">
    <div v-if="showFullTextSearch" class="collectionsearch-fulltext">
        <div ref="searchContainerElement" class="content">
            <div class="search-fulltext">
                <TextBox v-model="query" placeholder="What can we help you find?">
                    <template #prepend>
                        <div class="input-group-addon">
                            <i class="fa fa-search"></i>
                        </div>
                    </template>
                    <template #append>
                        <div v-if="query" class="input-group-addon">
                            <i class="fa fa-times clickable" @click="onClearClick"></i>
                        </div>
                    </template>
                </TextBox>
            </div>
        </div>
    </div>

    <FiltersContainer v-if="showFiltersPanel" :filters="filters" v-model:filterValues="filterValues" />

    <div class="collectionsearch-results">
        <div class="results-header">
            <div class="results-count">
                Results <span class="results-count-number">{{ totalResultsCount }}</span>
            </div>

            <div v-if="showSort" class="results-order">
                <DropDownList v-model="sortOrder" :items="sortOrderItems" :showBlankItem="false" />
            </div>
        </div>

        <div ref="searchResultContainerElement">
        </div>
    </div>
</div>
`
});
