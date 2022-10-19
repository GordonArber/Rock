<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <Alert v-if="blockErrorMessage"
           alertType="warning">
        {{ blockErrorMessage }}
    </Alert>

    <Panel v-else-if="isPanelVisible" type="block" title="Multiple Occurrences">
        <SectionHeader title="Multiple Occurrences"
                       description="There are multiple experience occurrences happening right now. Please select the one you would like to manage." />

        <div v-for="occurrence in occurrences" class="occurrence-item clickable" @click="onOccurrenceClick(occurrence)">
            <div class="occurrence-item-icon">
                <span class="icon">
                    <i class="fa fa-calendar-alt"></i>
                </span>
            </div>

            <div class="occurrence-item-content">
                {{ occurrence.text }}
            </div>

            <div class="occurrence-item-action">
                <span class="icon">
                    <i class="fa fa-arrow-circle-right"></i>
                </span>
            </div>
        </div>
    </Panel>
</template>

<style scoped>
.occurrence-item {
    display: flex;
    align-items: stretch;
    margin-bottom: 12px;
}

.occurrence-item > * {
    display: flex;
    align-items: center;
    align-self: stretch;
    padding: 8px 12px;
    border-top: 1px solid #c4c4c4;
    border-bottom: 1px solid #c4c4c4;
}

.occurrence-item > *:first-child {
    border-left: 1px solid #c4c4c4;
    border-radius: 8px 0px 0px 8px;
    background-color: var(--brand-info);
    color: white;
    justify-content: center;
    padding: 8px 0px;
    min-width: 35px;
}

.occurrence-item > *:last-child {
    border-right: 1px solid #c4c4c4;
    border-radius: 0px 8px 8px 0px;
    padding-right: 16px;
}

.occurrence-item > .occurrence-item-content {
    flex: 1 0;
}

.occurrence-item > .occurrence-item-action {
    color: var(--brand-info);
}

.occurrence-item:hover {
    background-color: rgba(85,150,230,0.1);
}
</style>

<script setup lang="ts">
    import Alert from "@Obsidian/Controls/alert.vue";
    import Panel from "@Obsidian/Controls/panel";
    import SectionHeader from "@Obsidian/Controls/sectionHeader";
    import { useConfigurationValues } from "@Obsidian/Utility/block";
    import { OccurrenceChooserInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceOccurrenceChooser/occurrenceChooserInitializationBox";
    import { computed, ref } from "vue";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { NavigationUrlKey } from "./InteractiveExperienceOccurrenceChooser/types";

    const config = useConfigurationValues<OccurrenceChooserInitializationBox>();

    // #region Values

    const isPanelVisible = ref(true);

    // #endregion

    // #region Computed Values

    const blockErrorMessage = computed((): string | undefined | null => {
        return config.errorMessage;
    });

    const occurrences = computed((): ListItemBag[] => {
        return config.occurrences ?? [];
    });

    // #endregion

    // #region Functions

    // #endregion

    // #region Event Handlers

    /**
     * Event handler for when the person clicks on one of the occurrences.
     * Navigate to the manager page for this occurrence.
     *
     * @param occurrence The occurrence to be managed.
     */
    function onOccurrenceClick(occurrence: ListItemBag): void {
        const urlTemplate = config.navigationUrls?.[NavigationUrlKey.ExperienceManagerPage];

        if (!urlTemplate || !occurrence.value) {
            return;
        }

        const url = urlTemplate.replace("((Id))", occurrence.value);

        window.location.href = url;
    }

    // #endregion

    // If only one occurrence, redirect to the manager page. This should be
    // replaced with a server-side redirect once that is possible.
    if (occurrences.value.length === 1) {
        isPanelVisible.value = false;
        onOccurrenceClick(occurrences.value[0]);
    }
</script>
