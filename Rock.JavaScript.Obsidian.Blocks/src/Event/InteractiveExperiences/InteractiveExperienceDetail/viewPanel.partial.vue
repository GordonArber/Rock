<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div class="row">
        <div class="col-md-6">
            <textarea v-model="ical" style="width: 100%; height: 400px;" />
        </div>

        <div class="col-md-6">
            <div v-html="icalFriendly"></div>

            <pre>{{ icalText }}</pre>
        </div>
    </div>

    <fieldset>
        <ValueDetailList :modelValue="topValues" />

        <div class="row">
            <div class="col-md-6">
                <ValueDetailList :modelValue="leftSideValues" />
            </div>

            <div class="col-md-6">
                <ValueDetailList :modelValue="rightSideValues" />
            </div>
        </div>

        <AttributeValuesContainer :modelValue="attributeValues" :attributes="attributes" :numberOfColumns="2" />
    </fieldset>
</template>

<script setup lang="ts">
    import { computed, PropType, ref, watch } from "vue";
    import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
    import ValueDetailList from "@Obsidian/Controls/valueDetailList";
    import { ValueDetailListItemBuilder } from "@Obsidian/Core/Controls/valueDetailListItemBuilder";
    import { ValueDetailListItem } from "@Obsidian/Types/Controls/valueDetailListItem";
    import { InteractiveExperienceBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceBag";
    import { InteractiveExperienceDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceDetailOptionsBag";
    import { ScheduleRuleBuilder } from "./scheduleHelper";
    import { RockDateTime } from "@Obsidian/Utility/rockDateTime";

    const ical = ref("");
    const icalFriendly = ref("");
    const icalText = ref("");

    watch(ical, () => {
        if (!ical.value) {
            return;
        }

        try {
            const sched = new ScheduleRuleBuilder(ical.value);
            const dates = sched.getDates(RockDateTime.fromParts(2000, 1, 1)!, RockDateTime.now().addYears(2));

            icalFriendly.value = sched.toFriendlyHtml()

            icalText.value = dates.map(d => d.toASPString("M/d/yyyy h:mm tt")).join("\n");
        }
        catch {
            icalFriendly.value = "";
            icalText.value = "";
        }
    });

    const props = defineProps({
        modelValue: {
            type: Object as PropType<InteractiveExperienceBag | null>,
            required: false
        },

        options: {
            type: Object as PropType<InteractiveExperienceDetailOptionsBag>,
            required: true
        }
    });

    // #region Values

    const attributes = ref(props.modelValue?.attributes ?? {});
    const attributeValues = ref(props.modelValue?.attributeValues ?? {});

    // #endregion

    // #region Computed Values

    /** The values to display full-width at the top of the block. */
    const topValues = computed((): ValueDetailListItem[] => {
        const valueBuilder = new ValueDetailListItemBuilder();

        if (!props.modelValue) {
            return valueBuilder.build();
        }

        if (props.modelValue.description) {
            valueBuilder.addTextValue("Description", props.modelValue.description);
        }

        return valueBuilder.build();
    });

    /** The values to display at half-width on the left side of the block. */
    const leftSideValues = computed((): ValueDetailListItem[] => {
        const valueBuilder = new ValueDetailListItemBuilder();

        if (!props.modelValue) {
            return valueBuilder.build();
        }

        return valueBuilder.build();
    });

    /** The values to display at half-width on the left side of the block. */
    const rightSideValues = computed((): ValueDetailListItem[] => {
        const valueBuilder = new ValueDetailListItemBuilder();

        if (!props.modelValue) {
            return valueBuilder.build();
        }

        return valueBuilder.build();
    });

    // #endregion

    // #region Functions

    // #endregion

    // #region Event Handlers

    // #endregion
</script>
