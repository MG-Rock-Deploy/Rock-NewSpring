<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <template v-if="allowAdd && isShowingAddForm">
        <RockLabel :help="help">{{ label }}</RockLabel>
        <Loading :isLoading="isLoading" class="well">
            <Alert v-if="fetchError" alertType="danger">Error: {{ fetchError }}</Alert>
            <Alert v-else-if="saveError" alertType="danger">Error: {{saveError}}</Alert>

            <RockForm v-else @submit="saveNewValue">
                <TextBox label="Value" v-model="newValue" rules="required" />
                <TextBox label="Description" v-model="newDescription" textMode="multiline" />
                <AttributeValuesContainer v-if="attributes != null" v-model="attributeValues" :attributes="attributes" isEditMode :showCategoryLabel="false" />
                <RockButton type="submit" :btnType="saveButtonType" :btnSize="formButtonSize">Add</RockButton>
                <RockButton :btnType="cancelButtonType" :btnSize="formButtonSize" @click="hideAddForm">Cancel</RockButton>
            </RockForm>

            <RockButton v-if="fetchError || saveError" :btnType="cancelButtonType" :btnSize="formButtonSize" @click="hideAddForm">Cancel</RockButton>
        </Loading>
    </template>

    <template v-else>
        <BaseAsyncPicker v-model="internalValue" v-bind="standardProps" :items="itemsSource" />
        <RockButton v-if="allowAdd && multiple && !enhanceForLongLists" @click="showAddForm" :btnType="addItemButtonType">Add Item</RockButton>
        <RockButton v-if="allowAdd && (!multiple || enhanceForLongLists)" @click="showAddForm" :btnType="plusButtonType" aria-label="Add Item"><i class="fa fa-plus" aria-hidden></i></RockButton>
    </template>
</template>

<script setup lang="ts">
    import { Guid } from "@Obsidian/Types";
    import { useSecurityGrantToken } from "@Obsidian/Utility/block";
    import { standardAsyncPickerProps, useStandardAsyncPickerProps, useVModelPassthrough } from "@Obsidian/Utility/component";
    import { useHttp } from "@Obsidian/Utility/http";
    import { DefinedValuePickerGetDefinedValuesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/definedValuePickerGetDefinedValuesOptionsBag";
    import { DefinedValuePickerSaveNewValueOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/definedValuePickerSaveNewValueOptionsBag";
    import { DefinedValuePickerGetAttributesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/definedValuePickerGetAttributesOptionsBag";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { PropType, ref, watch } from "vue";
    import BaseAsyncPicker from "./baseAsyncPicker";
    import RockLabel from "./rockLabel";
    import RockForm from "./rockForm";
    import AttributeValuesContainer from "./attributeValuesContainer";
    import TextBox from "./textBox";
    import RockButton from "./rockButton";
    import Loading from "./loading";
    import Alert from "./alert.vue";
    import { BtnSize, BtnType } from "@Obsidian/Enums/Controls/buttonOptions";
    import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

    const props = defineProps({
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        definedTypeGuid: {
            type: String as PropType<Guid>,
            required: false
        },

        allowAdd: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        ...standardAsyncPickerProps
    });

    const emit = defineEmits<{
        (e: "update:modelValue", _value: ListItemBag | ListItemBag[] | null): void
    }>();

    const http = useHttp();

    // #region Picker

    const standardProps = useStandardAsyncPickerProps(props);
    const securityGrantToken = useSecurityGrantToken();
    const internalValue = useVModelPassthrough(props, "modelValue", emit);
    const itemsSource = ref<(() => Promise<ListItemBag[]>) | null>(null);

    async function loadItems(): Promise<ListItemBag[]> {
        const options: Partial<DefinedValuePickerGetDefinedValuesOptionsBag> = {
            definedTypeGuid: props.definedTypeGuid,
            securityGrantToken: securityGrantToken.value
        };
        const url = "/api/v2/Controls/DefinedValuePickerGetDefinedValues";
        const result = await http.post<ListItemBag[]>(url, undefined, options);

        if (result.isSuccess && result.data) {
            return result.data;
        }
        else {
            console.error(result.errorMessage ?? "Unknown error while loading data.");
            return [];
        }
    }

    function fetchValues(): void {
        itemsSource.value = () => loadItems();
    }

    watch(() => props.definedTypeGuid, fetchValues);

    fetchValues();

    // #endregion

    // #region Add Value

    const isShowingAddForm = ref(false);
    const isLoading = ref(false);
    const fetchError = ref<false | string>(false);
    const saveError = ref<false | string>(false);

    async function showAddForm(): Promise<void> {
        if (!props.allowAdd) return;

        isShowingAddForm.value = true;

        if (attributes.value == null) {
            isLoading.value = true;
            fetchError.value = false;
            saveError.value = false;

            const options: Partial<DefinedValuePickerGetAttributesOptionsBag> = {
                definedTypeGuid: props.definedTypeGuid,
                securityGrantToken: securityGrantToken.value
            };
            const url = "/api/v2/Controls/DefinedValuePickerGetAttributes";
            const result = await http.post<PublicAttributeBag[]>(url, undefined, options);

            if (result.isSuccess && result.data) {
                attributes.value = result.data.reduce(function (acc, val) {
                    acc[val.key as string] = val;
                    return acc;
                }, {});
            }
            else {
                attributes.value = null;
                fetchError.value = "Unable to fetch attribute data.";
            }

            isLoading.value = false;
        }
    }

    function hideAddForm(): void {
        isShowingAddForm.value = false;
        fetchError.value = false;
        saveError.value = false;
    }

    async function saveNewValue(): Promise<void> {
        isLoading.value = true;
        saveError.value = false;

        const options: Partial<DefinedValuePickerSaveNewValueOptionsBag> = {
            definedTypeGuid: props.definedTypeGuid,
            securityGrantToken: securityGrantToken.value,
            value: newValue.value,
            description: newDescription.value,
            attributeValues: attributeValues.value
        };
        const url = "/api/v2/Controls/DefinedValuePickerSaveNewValue";
        const result = await http.post<ListItemBag>(url, undefined, options);

        if (result.isSuccess && result.data) {
            fetchValues();

            if (props.multiple) {
                if (Array.isArray(internalValue.value)) {
                    internalValue.value.push(result.data);
                }
                else {
                    internalValue.value = [result.data];
                }
            }
            else {
                internalValue.value = result.data;
            }

            hideAddForm();
            newValue.value = "";
            newDescription.value = "";
            attributeValues.value = {};
        }
        else {
            saveError.value = "Unable to save new Defined Value.";
        }

        isLoading.value = false;
    }


    const attributes = ref<Record<string, PublicAttributeBag> | null>(null);
    const attributeValues = ref<Record<string, string>>({});
    const newValue = ref("");
    const newDescription = ref("");

    const saveButtonType = BtnType.Primary;
    const cancelButtonType = BtnType.Link;
    const formButtonSize = BtnSize.ExtraSmall;
    const plusButtonType = BtnType.Default;
    const addItemButtonType = BtnType.Link;

    // #endregion
</script>
