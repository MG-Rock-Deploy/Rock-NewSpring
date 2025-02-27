<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <ContentDropDownPicker
                           v-bind="formFieldProps"
                           :modelValue="pickerValue"
                           iconCssClass="fa fa-map-marker"
                           pickerClass="picker-geography"
                           :showClear="!!pickerValue"
                           primaryButtonLabel="Done"
                           v-model:isFullscreen="isFullscreen"
                           @primaryButtonClicked="select"
                           @secondaryButtonClicked="cancel"
                           @clearButtonClicked="clear"
                           @update:showPopup="toggledPopup"
                           pickerContentBoxHeight="auto"
                           pickerContentHeadingText="Geography Picker"
                           disablePickerContentBoxScroll
                           showFullscreenButton>

        <template #innerLabel>
            <span class="selected-names" v-html="pickerLabel"></span>
        </template>

        <GeoPickerMap
                      class="geo-picker-map"
                      v-model="mapValue"
                      :drawingMode="drawingMode"
                      :isExpanded="isFullscreen"
                      :mapStyleValueGuid="mapStyleValueGuid"
                      :centerLatitude="centerLatitude"
                      :centerLongitude="centerLongitude" />
    </ContentDropDownPicker>
</template>

<script setup lang="ts">
    import { PropType, ref, watch, onBeforeMount } from "vue";
    import { standardRockFormFieldProps, useStandardRockFormFieldProps } from "@Obsidian/Utility/component";
    import ContentDropDownPicker from "@Obsidian/Controls/contentDropDownPicker.vue";
    import GeoPickerMap from "@Obsidian/Controls/geoPickerMap.vue";
    import { DrawingMode } from "@Obsidian/Types/Controls/geo";
    import { wellKnownToCoordinates, nearAddressForCoordinates, loadMapResources } from "@Obsidian/Utility/geo";
    import { Guid } from "@Obsidian/Types";
    import { DefinedValue } from "@Obsidian/SystemGuids";

    const props = defineProps({
        ...standardRockFormFieldProps,

        /**
         * Geographical Point or Polygon coordinates in Well Known Text format
         */
        modelValue: {
            type: String as PropType<string>,
            default: ""
        },

        /**
         * GUID of a DefinedValue of the Map Styles DefinedType. Determines the way the map looks.
         */
        mapStyleValueGuid: {
            type: String as PropType<Guid>,
            default: DefinedValue.MapStyleRock
        },

        /**
         * Latitude coordinate to center map on if not initialized with a shape. This prop is not reactive.
         */
        centerLatitude: {
            type: Number as PropType<number>,
            default: null
        },

        /**
         * Longitude coordinate to center map on if not initialized with a shape. This prop is not reactive.
         */
        centerLongitude: {
            type: Number as PropType<number>,
            default: null
        },

        /**
         * What are we drawing? Point or Polygon? This prop is not reactive.
         */
        drawingMode: {
            type: String as PropType<DrawingMode>,
            required: true
        },
    });

    const emit = defineEmits<{
        (e: "update:modelValue", value: string): void
    }>();

    // #region Values

    const mapValue = ref(props.modelValue);

    const pickerValue = ref(props.modelValue);
    const pickerLabel = ref("");

    const isFullscreen = ref(false);
    const formFieldProps = useStandardRockFormFieldProps(props);

    // #endregion

    // #region Functions

    async function getAddress(): Promise<void> {
        if (pickerValue.value) {
            pickerLabel.value = "<i>Selected</i>"; // temporarily while we get the new value
            const address = await nearAddressForCoordinates(wellKnownToCoordinates(pickerValue.value, props.drawingMode));
            if (address) {
                pickerLabel.value = address;
            }
        }
        else {
            pickerLabel.value = "";
        }
    }

    // #endregion

    // #region Event Handlers

    function select(): void {
        pickerValue.value = mapValue.value;
    }

    function cancel(): void {
        // Reset the map values to the picker values when selection is cancelled
        mapValue.value = pickerValue.value;
    }

    function clear(): void {
        pickerValue.value = "";
        pickerLabel.value = "";
    }

    // If we close the popup when it was fullscreen, take it out of fullscreen
    function toggledPopup(isShowing): void {
        if (!isShowing) {
            isFullscreen.value = false;
        }
    }

    // #endregion

    // #region Watchers

    watch(() => props.modelValue, () => {
        mapValue.value = props.modelValue;
        pickerValue.value = props.modelValue;
    });

    watch(pickerValue, () => {
        emit("update:modelValue", pickerValue.value);
        getAddress();
    });

    // #endregion

    // #region Life Cycle

    /*
    * Load Google Maps and grab data needed from the server
    */
    onBeforeMount(async (): Promise<void> => {
        await loadMapResources();
        getAddress();
    });

    // #endregion
</script>
