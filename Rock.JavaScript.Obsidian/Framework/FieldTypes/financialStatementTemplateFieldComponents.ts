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

import { defineComponent, ref, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import FinancialStatementTemplatePicker from "@Obsidian/Controls/financialStatementTemplatePicker.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { updateRefValue } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "FinancialStatementTemplateField.Edit",

    components: {
        FinancialStatementTemplatePicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<ListItemBag | null>();

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue ? JSON.parse(props.modelValue) : null);
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value ?? ""));
        });

        return {
            internalValue
        };
    },

    template: `
    <FinancialStatementTemplatePicker showBlankItem v-model="internalValue" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "FinancialStatementTemplateField.Configuration",

    template: ``
});

