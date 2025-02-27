<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <RockFormField v-bind="standardFieldProps" name="maxAge" :modelValue="internalValue">
        <div class="btn-group">
            <RockButton v-for="(item, index) in items" @click="internalValue = item.value" :btnSize="btnSize" :btnType="itemButtonType(item)">{{item.text}}</RockButton>
        </div>
    </RockFormField>
</template>

<script setup lang="ts">
    import { PropType, computed } from "vue";
    import RockFormField from "@Obsidian/Controls/rockFormField";
    import RockButton from "@Obsidian/Controls/rockButton";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { BtnType, BtnSize } from "@Obsidian/Enums/Controls/buttonOptions";
    import { standardRockFormFieldProps, useStandardRockFormFieldProps } from "@Obsidian/Utility/component";

    // Use ListItemBag, but don't worry about the "category" property and make the other properties not null/undefined
    type StandardListItemBag = {
        [P in keyof Pick<ListItemBag, "text" | "value">]-?: NonNullable<ListItemBag[P]>;
    };

    const props = defineProps({
        modelValue: {
            type: Object as PropType<string>,
            required: true
        },

        items: {
            type: Array as PropType<StandardListItemBag[]>,
            default: () => []
        },

        unselectedBtnType: {
            type: String as PropType<BtnType>,
            default: BtnType.Default
        },

        selectedBtnType: {
            type: String as PropType<BtnType>,
            default: BtnType.Primary
        },

        btnSize: {
            type: String as PropType<BtnSize>,
            default: BtnSize.Default
        },

        ...standardRockFormFieldProps
    });

    const emit = defineEmits<{
        (e: "update:modelValue", _value: string): void
    }>();

    const internalValue = computed<string>({
        get() {
            return props.modelValue;
        },
        set(newValue) {
            emit("update:modelValue", newValue);
        }
    });

    function itemButtonType(item: StandardListItemBag): BtnType {
        return item.value == internalValue.value ? props.selectedBtnType : props.unselectedBtnType;
    }

    const standardFieldProps = useStandardRockFormFieldProps(props);
</script>
