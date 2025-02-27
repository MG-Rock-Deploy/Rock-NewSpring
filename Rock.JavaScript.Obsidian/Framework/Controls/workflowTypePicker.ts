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

import { useSecurityGrantToken } from "@Obsidian/Utility/block";
import { standardAsyncPickerProps, updateRefValue } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { defineComponent, PropType, ref, watch } from "vue";
import { WorkflowTypeTreeItemProvider } from "@Obsidian/Utility/treeItemProviders";
import RockFormField from "./rockFormField";
import TreeItemPicker from "./treeItemPicker";

export default defineComponent({
    name: "WorkflowTypePicker",

    components: {
        TreeItemPicker,
        RockFormField
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        includeInactiveItems: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        ...standardAsyncPickerProps
    },

    emits: {
        "update:modelValue": (_value: ListItemBag | ListItemBag[] | null) => true
    },

    setup(props, { emit }) {
        // #region Values

        const internalValue = ref(props.modelValue ?? null);
        const securityGrantToken = useSecurityGrantToken();

        const itemProvider = ref(new WorkflowTypeTreeItemProvider());
        itemProvider.value.includeInactiveItems = props.includeInactiveItems;
        itemProvider.value.securityGrantToken = securityGrantToken.value;

        // #endregion

        // #region Watchers

        // Keep security token up to date, but don't need refetch data
        watch(securityGrantToken, () => {
            itemProvider.value.securityGrantToken = securityGrantToken.value;
        });

        // When this changes, we need to refetch the data, so reset the whole itemProvider
        watch(() => props.includeInactiveItems, () => {
            const oldProvider = itemProvider.value;
            const newProvider = new WorkflowTypeTreeItemProvider();

            // copy old provider's properties
            newProvider.securityGrantToken = oldProvider.securityGrantToken;
            // Use new value
            newProvider.includeInactiveItems = props.includeInactiveItems;

            // Set the provider to the new one
            itemProvider.value = newProvider;
        });

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue ?? null);
        });

        // #endregion

        return {
            internalValue,
            itemProvider
        };
    },
    template: `
<TreeItemPicker v-model="internalValue"
    formGroupClasses="category-picker"
    iconCssClass="fa fa-cogs"
    :provider="itemProvider"
    :multiple="multiple"
    disableFolderSelection
/>
`
});
