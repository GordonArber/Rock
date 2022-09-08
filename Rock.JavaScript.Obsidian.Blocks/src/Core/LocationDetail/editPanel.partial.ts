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

import { computed, defineComponent, PropType, ref, watch } from "vue";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import AddressControl from "@Obsidian/Controls/addressControl";
import CheckBox from "@Obsidian/Controls/checkBox";
import DefinedValuePicker from "@Obsidian/Controls/definedValuePicker";
import DropDownList from "@Obsidian/Controls/dropDownList";
import ImageUploader from "@Obsidian/Controls/imageUploader";
import LocationPicker from "@Obsidian/Controls/locationPicker";
import NumberBox from "@Obsidian/Controls/numberBox";
import TextBox from "@Obsidian/Controls/textBox";
import { watchPropertyChanges } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { LocationBag } from "@Obsidian/ViewModels/Blocks/Core/LocationDetail/locationBag";
import { LocationDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Core/LocationDetail/locationDetailOptionsBag";
import { DefinedType } from "../../../../Rock.JavaScript.Obsidian/Framework/SystemGuids";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export default defineComponent({
    name: "Core.LocationDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<LocationBag>,
            required: true
        },

        options: {
            type: Object as PropType<LocationDetailOptionsBag>,
            required: true
        }
    },

    components: {
        AddressControl,
        AttributeValuesContainer,
        CheckBox,
        DefinedValuePicker,
        DropDownList,
        ImageUploader,
        LocationPicker,
        NumberBox,
        TextBox
    },

    emits: {
        "update:modelValue": (_value: LocationBag) => true,
        "propertyChanged": (_value: string) => true
    },

    setup(props, { emit }) {
        // #region Values
        debugger;
        const attributes = ref(props.modelValue.attributes ?? {});
        const attributeValues = ref(props.modelValue.attributeValues ?? {});
        const parentLocation = propertyRef(props.modelValue.parentLocation ?? null, "ParentLocationId");
        const isActive = propertyRef(props.modelValue.isActive ?? false, "IsActive");
        const name = propertyRef(props.modelValue.name ?? "", "Name");
        const locationTypeValue = propertyRef(props.modelValue.locationTypeValue ?? null, "LocationTypeValueId");
        const printerDeviceId = propertyRef(props.modelValue.printerDeviceId ?? null, "PrinterDeviceId");
        const isGeoPointLocked = propertyRef(props.modelValue.isGeoPointLocked ?? false, "IsGeoPointLocked");
        const softRoomThreshold = propertyRef(props.modelValue.softRoomThreshold ?? null, "SoftRoomThreshold");
        const firmRoomThreshold = propertyRef(props.modelValue.firmRoomThreshold ?? null, "FirmRoomThreshold");
        const addressFields = ref(props.modelValue ?? {});

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [isActive,
            name,
            parentLocation,
            locationTypeValue,
            printerDeviceId,
            isGeoPointLocked,
            softRoomThreshold,
            firmRoomThreshold
        ];

        // #endregion

        // #region Computed Values

        const printerDeviceOptions = computed((): ListItemBag[] => {
            return props.options.printerDeviceOptions ?? [];
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        // Watch for parental changes in our model value and update all our values.
        watch(() => props.modelValue, () => {
            updateRefValue(addressFields, props.modelValue);
            updateRefValue(attributes, props.modelValue.attributes ?? {});
            updateRefValue(attributeValues, props.modelValue.attributeValues ?? {});
            updateRefValue(parentLocation, props.modelValue.parentLocation ?? null);
            updateRefValue(isActive, props.modelValue.isActive ?? false);
            updateRefValue(name, props.modelValue.name ?? "");
            updateRefValue(locationTypeValue, props.modelValue.locationTypeValue ?? null);
            updateRefValue(printerDeviceId, props.modelValue.printerDeviceId ?? null);

            updateRefValue(isGeoPointLocked, props.modelValue.isGeoPointLocked ?? false);
            updateRefValue(softRoomThreshold, props.modelValue.softRoomThreshold ?? null) ;
            updateRefValue(firmRoomThreshold, props.modelValue.firmRoomThreshold ?? null);
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([attributeValues, ...propRefs], () => {
            const newValue: LocationBag = {
                ...props.modelValue,
                attributeValues: attributeValues.value,
                isActive: isActive.value,
                name: name.value,
                locationTypeValue: locationTypeValue.value,
                parentLocation: parentLocation.value,
                printerDeviceId: printerDeviceId.value,
                isGeoPointLocked: isGeoPointLocked.value,
                softRoomThreshold: softRoomThreshold.value,
                firmRoomThreshold: firmRoomThreshold.value
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any changes to props that represent properties and then
        // automatically emit which property changed.
        watchPropertyChanges(propRefs, emit);

        return {
            attributes,
            attributeValues,
            isActive,
            name,
            locationTypeValue,
            locationTypeDefinedTypeGuid: DefinedType.LocationType,
            parentLocation,
            printerDeviceId,
            printerDeviceOptions,
            isGeoPointLocked,
            softRoomThreshold,
            firmRoomThreshold
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">

            <LocationPicker v-model="parentLocation"
                label="Parent Location" />

            <TextBox v-model="name"
                label="Name"
                rules="required" />

            <DefinedValuePicker v-model="locationTypeValue"
                label="Location Type"
                showBlankItem
                :definedTypeGuid="locationTypeDefinedTypeGuid" />

            <DropDownList v-model="printerDeviceId"
                label="Printer #TODO#"
                help="The printer that this location should use for printing."
                :items="printerDeviceOptions" />
        </div>

        <div class="col-md-6">
            <CheckBox v-model="isActive"
                label="Active" />

            <AddressControl label="" v-model="addressFields" />
        </div>
    </div>

    <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
</fieldset>
`
});
