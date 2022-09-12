<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <CheckBoxList v-if="multiple" v-model="(internalValue as string[])" :items="options" />
    <DropDownList v-else v-model="internalValue" :items="options" />
</template>

<script setup lang="ts">
    import { PropType, ref, watch } from 'vue';
    import { DayOfWeek } from '@Obsidian/Enums/Controls/dayOfWeek';
    import { ListItemBag } from '@Obsidian/ViewModels/Utility/listItemBag';
    import { useVModelPassthrough } from '@Obsidian/Utility/component';
    import { toNumber } from '@Obsidian/Utility/numberUtils';
    import DropDownList from './dropDownList';
    import CheckBoxList from './checkBoxList';

    const props = defineProps({
        modelValue: {
            type: String as PropType<string | string[] | null>,
            default: null
        },
        multiple: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    });

    const emit = defineEmits<{
        (e: "update:modelValue", _value: string): void
    }>();

    const internalValue = useVModelPassthrough(props, "modelValue", emit);



    const options: ListItemBag[] = [
        { text: "Sunday", value: DayOfWeek.Sunday.toString() },
        { text: "Monday", value: DayOfWeek.Monday.toString() },
        { text: "Tuesday", value: DayOfWeek.Tuesday.toString() },
        { text: "Wednesday", value: DayOfWeek.Wednesday.toString() },
        { text: "Thursday", value: DayOfWeek.Thursday.toString() },
        { text: "Friday", value: DayOfWeek.Friday.toString() },
        { text: "Saturday", value: DayOfWeek.Saturday.toString() }
    ];
</script>