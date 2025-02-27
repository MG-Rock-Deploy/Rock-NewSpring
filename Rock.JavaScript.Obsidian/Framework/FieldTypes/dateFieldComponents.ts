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
import { computed, defineComponent, ref, watch } from "vue";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { toNumber } from "@Obsidian/Utility/numberUtils";
import { ConfigurationValueKey } from "./dateField.partial";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import SlidingDateRangePicker from "@Obsidian/Controls/slidingDateRangePicker";
import DatePicker from "@Obsidian/Controls/datePicker";
import DatePartsPicker, { getDefaultDatePartsPickerModel } from "@Obsidian/Controls/datePartsPicker";
import DropDownList from "@Obsidian/Controls/dropDownList";
import TextBox from "@Obsidian/Controls/textBox";
import NumberBox from "@Obsidian/Controls/numberBox";
import CheckBox from "@Obsidian/Controls/checkBox";
import { PropType } from "vue";
import { ComparisonType } from "@Obsidian/Types/Reporting/comparisonType";
import { parseSlidingDateRangeString, slidingDateRangeToString } from "@Obsidian/Utility/slidingDateRange";
import { updateRefValue } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "DateField.Edit",

    components: {
        DatePicker,
        DatePartsPicker
    },

    props: getFieldEditorProps(),

    data() {
        return {
            internalValue: "",
            internalDateParts: getDefaultDatePartsPickerModel(),
            formattedString: ""
        };
    },

    setup() {
        return {
        };
    },

    computed: {
        datePartsAsDate(): RockDateTime | null {
            if (!this.internalDateParts?.day || !this.internalDateParts.month || !this.internalDateParts.year) {
                return null;
            }

            return RockDateTime.fromParts(this.internalDateParts.year, this.internalDateParts.month, this.internalDateParts.day) || null;
        },

        isDatePartsPicker(): boolean {
            const config = this.configurationValues[ConfigurationValueKey.DatePickerControlType];
            return config?.toLowerCase() === "date parts picker";
        },

        configAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};

            const displayCurrentConfig = this.configurationValues[ConfigurationValueKey.DisplayCurrentOption];
            const displayCurrent = asBoolean(displayCurrentConfig);
            attributes.displayCurrentOption = displayCurrent;
            attributes.isCurrentDateOffset = displayCurrent;

            const futureYearConfig = this.configurationValues[ConfigurationValueKey.FutureYearCount];
            const futureYears = toNumber(futureYearConfig);

            if (futureYears > 0) {
                attributes.futureYearCount = futureYears;
            }

            return attributes;
        }
    },

    methods: {
        syncModelValue(): void {
            this.internalValue = this.modelValue ?? "";
            const dateParts = /^(\d{4})-(\d{1,2})-(\d{1,2})/.exec(this.modelValue ?? "");

            if (dateParts != null) {
                this.internalDateParts.year = toNumber(dateParts[1]);
                this.internalDateParts.month = toNumber(dateParts[2]);
                this.internalDateParts.day = toNumber(dateParts[3]);
            }
            else {
                this.internalDateParts.year = 0;
                this.internalDateParts.month = 0;
                this.internalDateParts.day = 0;
            }
        }
    },

    watch: {
        datePartsAsDate(): void {
            if (this.isDatePartsPicker) {
                const d1 = this.datePartsAsDate;
                const d2 = RockDateTime.parseISO(this.modelValue ?? "");

                if (d1 === null || d2 === null || !d1.isEqualTo(d2)) {
                    this.$emit("update:modelValue", d1 !== null ? d1.toISOString().split("T")[0] : "");
                }
            }
        },

        internalValue(): void {
            if (!this.isDatePartsPicker) {
                const d1 = RockDateTime.parseISO(this.internalValue);
                const d2 = RockDateTime.parseISO(this.modelValue ?? "");

                if (d1 === null || d2 === null || !d1.isEqualTo(d2)) {
                    this.$emit("update:modelValue", this.internalValue);
                }
            }
        },

        modelValue: {
            immediate: true,
            async handler(): Promise<void> {
                await this.syncModelValue();
            }
        }
    },

    template: `
<DatePartsPicker v-if="isDatePartsPicker" v-model="internalDateParts" v-bind="configAttributes" />
<DatePicker v-else v-model="internalValue" v-bind="configAttributes" />
`
});

export const FilterComponent = defineComponent({
    name: "DateField.Filter",

    components: {
        EditComponent,
        SlidingDateRangePicker
    },

    props: {
        ...getFieldEditorProps(),
        comparisonType: {
            type: Number as PropType<ComparisonType | null>,
            required: true
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        // The internal values that make up the model value.
        const internalValue = ref(props.modelValue);
        const internalValueSegments = internalValue.value.split("\t");
        const dateValue = ref(internalValueSegments[0]);
        const rangeValue = ref(parseSlidingDateRangeString(internalValueSegments.length > 1 ? internalValueSegments[1] : ""));

        // Get the configuration values and force the DisplayCurrentOption to True.
        const configurationValues = ref({ ...props.configurationValues });
        configurationValues.value[ConfigurationValueKey.DisplayCurrentOption] = "True";

        /** True if the comparison type is of type Between. */
        const isComparisonTypeBetween = computed((): boolean => props.comparisonType === ComparisonType.Between);

        // Watch for changes in the configuration values and update our own list.
        watch(() => props.configurationValues, () => {
            configurationValues.value = { ...props.configurationValues };
            configurationValues.value[ConfigurationValueKey.DisplayCurrentOption] = "True";
        });

        // Watch for changes from the standard DatePicker.
        watch(dateValue, () => {
            if (props.comparisonType !== ComparisonType.Between) {
                internalValue.value = `${dateValue.value}\t`;
            }
        });

        // Watch for changes from the SlidingDateRangePicker.
        watch(rangeValue, () => {
            if (props.comparisonType === ComparisonType.Between) {
                internalValue.value = `\t${rangeValue.value ? slidingDateRangeToString(rangeValue.value) : ""}`;
            }
        });

        // Watch for changes to the model value and update our internal values.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue;
            const segments = internalValue.value.split("\t");
            dateValue.value = segments[0];
            updateRefValue(rangeValue, parseSlidingDateRangeString(segments.length > 1 ? segments[1] : ""));
        });

        // Watch for changes to our internal value and update the model value.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            configurationValues,
            dateValue,
            isComparisonTypeBetween,
            rangeValue
        };
    },

    template: `
<SlidingDateRangePicker v-if="isComparisonTypeBetween" v-model="rangeValue" />
<EditComponent v-else v-model="dateValue" :configurationValues="configurationValues" />
`
});

const defaults = {
    [ConfigurationValueKey.Format]: "",
    [ConfigurationValueKey.DisplayDiff]: "False",
    [ConfigurationValueKey.DisplayCurrentOption]: "False",
    [ConfigurationValueKey.DatePickerControlType]: "Date Picker",
    [ConfigurationValueKey.FutureYearCount]: ""
};

export const ConfigurationComponent = defineComponent({
    name: "DateField.Configuration",

    components: {
        TextBox,
        CheckBox,
        DropDownList,
        NumberBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const format = ref("");
        const displayDiff = ref(false);
        const displayCurrentOption = ref(false);
        const pickerControlType = ref("Date Picker");
        const futureYears = ref<number | null>(null);

        const pickerControlTypeOptions = [
            { text: "Date Picker", value: "Date Picker" },
            { text: "Date Parts Picker", value: "Date Parts Picker" }
        ];

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {};

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.Format] = format.value ?? defaults[ConfigurationValueKey.Format];
            newValue[ConfigurationValueKey.DisplayDiff] = asTrueFalseOrNull(displayDiff.value) ?? defaults[ConfigurationValueKey.DisplayDiff];
            newValue[ConfigurationValueKey.DisplayCurrentOption] = asTrueFalseOrNull(displayCurrentOption.value) ?? defaults[ConfigurationValueKey.DisplayCurrentOption];
            newValue[ConfigurationValueKey.DatePickerControlType] = pickerControlType.value ?? defaults[ConfigurationValueKey.DatePickerControlType];
            newValue[ConfigurationValueKey.FutureYearCount] = futureYears.value?.toString() ?? defaults[ConfigurationValueKey.FutureYearCount];

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.Format] !== (props.modelValue[ConfigurationValueKey.Format] ?? defaults[ConfigurationValueKey.Format])
                || newValue[ConfigurationValueKey.DisplayDiff] !== (props.modelValue[ConfigurationValueKey.DisplayDiff] ?? defaults[ConfigurationValueKey.DisplayDiff])
                || newValue[ConfigurationValueKey.DisplayCurrentOption] !== (props.modelValue[ConfigurationValueKey.DisplayCurrentOption] ?? defaults[ConfigurationValueKey.DisplayCurrentOption])
                || newValue[ConfigurationValueKey.DatePickerControlType] !== (props.modelValue[ConfigurationValueKey.DatePickerControlType] ?? defaults[ConfigurationValueKey.DatePickerControlType])
                || newValue[ConfigurationValueKey.FutureYearCount] !== (props.modelValue[ConfigurationValueKey.FutureYearCount] ?? defaults[ConfigurationValueKey.FutureYearCount]);

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
                return true;
            }
            else {
                return false;
            }
        };

        /**
         * Emits the updateConfigurationValue if the value has actually changed.
         * 
         * @param key The key that was possibly modified.
         * @param value The new value.
         */
        const maybeUpdateConfiguration = (key: string, value: string): void => {
            if (maybeUpdateModelValue()) {
                emit("updateConfigurationValue", key, value);
            }
        };

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            format.value = props.modelValue[ConfigurationValueKey.Format] ?? "";
            displayDiff.value = asBoolean(props.modelValue[ConfigurationValueKey.DisplayDiff]);
            displayCurrentOption.value = asBoolean(props.modelValue[ConfigurationValueKey.DisplayCurrentOption]);
            pickerControlType.value = props.modelValue[ConfigurationValueKey.DatePickerControlType] ?? "Date Picker";
            futureYears.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.FutureYearCount]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        // THIS IS JUST A PLACEHOLDER FOR COPYING TO NEW FIELDS THAT MIGHT NEED IT.
        // THIS FIELD DOES NOT NEED THIS
        watch([], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(format, (val) => maybeUpdateConfiguration(ConfigurationValueKey.Format, val ?? defaults[ConfigurationValueKey.Format]));
        watch(displayDiff, (val) => maybeUpdateConfiguration(ConfigurationValueKey.DisplayDiff, asTrueFalseOrNull(val) ?? defaults[ConfigurationValueKey.DisplayDiff]));
        watch(displayCurrentOption, (val) => maybeUpdateConfiguration(ConfigurationValueKey.DisplayCurrentOption, asTrueFalseOrNull(val) ?? defaults[ConfigurationValueKey.DisplayCurrentOption]));
        watch(pickerControlType, (val) => maybeUpdateConfiguration(ConfigurationValueKey.DatePickerControlType, val || defaults[ConfigurationValueKey.DatePickerControlType]));
        watch(futureYears, (val) => maybeUpdateConfiguration(ConfigurationValueKey.FutureYearCount, val?.toString() ?? defaults[ConfigurationValueKey.FutureYearCount]));

        return {
            format,
            displayDiff,
            displayCurrentOption,
            pickerControlType,
            futureYears,
            pickerControlTypeOptions
        };
    },

    template: `
<div>
    <TextBox v-model="format" label="Date Format" help="The format string to use for date (default is system short date)" />
    <CheckBox v-model="displayDiff" label="Display as Elapsed Time" text="Yes" help="Display value as an elapsed time" />
    <DropDownList v-model="pickerControlType" :items="pickerControlTypeOptions" :show-blank-item="false" label="Control Type" help="Select 'Date Picker' to use a Date Picker, or 'Date Parts Picker' to select Month, Day, and Year individually" />
    <CheckBox v-if="pickerControlType == 'Date Picker'" v-model="displayCurrentOption" label="Display Current Option" text="Yes" help="Include option to specify value as the current date" />
    <NumberBox v-else v-model="futureYears"  label="Future Years" help="The number of years  in the future to include in the year picker. Set to 0 to limit to current year. Leaving it blank will default to 50." />
</div>
`
});
