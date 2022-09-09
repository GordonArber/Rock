<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <DropDownList v-model="internalValue" :items="options" />
</template>

<script setup lang="ts">
    import { PropType } from 'vue';
    import { ListItemBag } from '@Obsidian/ViewModels/Utility/listItemBag';
    import { useVModelPassthrough } from '@Obsidian/Utility/component';
    import DropDownList from './dropDownList';

    enum DayOfWeek {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6
    }

    const props = defineProps({
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
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