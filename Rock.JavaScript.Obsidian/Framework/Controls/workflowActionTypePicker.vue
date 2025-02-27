<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <TreeItemPicker v-model="internalValue"
                    formGroupClasses="workflow-action-type-picker"
                    iconCssClass="fa fa-folder"
                    :provider="itemProvider"
                    disableFolderSelection />
</template>

<script setup lang="ts">
    import { useSecurityGrantToken } from "@Obsidian/Utility/block";
    import { updateRefValue } from "@Obsidian/Utility/component";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { PropType, ref, watch } from "vue";
    import { WorkflowActionTypeTreeItemProvider } from "@Obsidian/Utility/treeItemProviders";
    import TreeItemPicker from "./treeItemPicker";

    const props = defineProps({
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },
    });

    const emit = defineEmits<{
        (e: "update:modelValue", _value: ListItemBag | ListItemBag[] | null): void
    }>();

    // #region Values

    const internalValue = ref(props.modelValue ?? null);
    const includeInactive = ref(false);
    const securityGrantToken = useSecurityGrantToken();
    const itemProvider = ref<WorkflowActionTypeTreeItemProvider>();

    // Configure the item provider with our settings.
    function refreshItemProvider(): void {
        const provider = new WorkflowActionTypeTreeItemProvider();
        provider.includeInactive = includeInactive.value;
        provider.securityGrantToken = securityGrantToken.value;

        itemProvider.value = provider;
    }

    refreshItemProvider();

    // #endregion

    // #region Watchers

    watch(includeInactive, refreshItemProvider);

    // Keep security token up to date, but don't need refetch data
    watch(securityGrantToken, () => {
        if (itemProvider.value) {
            itemProvider.value.securityGrantToken = securityGrantToken.value;
        }
    });

    watch(internalValue, () => {
        emit("update:modelValue", internalValue.value);
    });

    watch(() => props.modelValue, () => {
        updateRefValue(internalValue, props.modelValue ?? null);
    });

    // #endregion
</script>