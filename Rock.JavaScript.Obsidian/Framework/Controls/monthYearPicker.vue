<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <DatePartsPicker v-model="internalValue" hideDay />
</template>

<script setup lang="ts">
    import { PropType, computed } from 'vue';
    import DatePartsPicker, { DatePartsPickerValue, getDefaultDatePartsPickerModel } from './datePartsPicker';
    import { MonthYearValue } from "@Obsidian/ViewModels/Controls/monthYearValue";

    const props = defineProps({
        modelValue: {
            type: Object as PropType<MonthYearValue>,
            default: getDefaultDatePartsPickerModel
        }
    });

    const emit = defineEmits<{
        (e: "update:modelValue", _value: MonthYearValue): void
    }>();

    const internalValue = computed<DatePartsPickerValue>({
        get() {
            return Object.assign({}, props.modelValue, { day: 0 })
        },
        set(newVal) {
            const value: MonthYearValue = {
                month: newVal.month,
                year: newVal.year
            }
            emit("update:modelValue", value)
        }
    })
</script>
