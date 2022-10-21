<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <Alert v-if="config.errorMessage"
           alertType="warning">
        {{ config.errorMessage }}
    </Alert>

    <Panel v-else type="block" :title="panelTitle" hasFullscreen>
        <template #preBody>
            <PanelNavigationBar v-model="selectedTab"
                                :items="navigationTabs" />
        </template>

        <LiveEventTab v-show="isLiveEventTab" />
    </Panel>
</template>

<style scoped>

</style>

<script setup lang="ts">
    import Alert from "@Obsidian/Controls/alert.vue";
    import LiveEventTab from "./ExperienceManager/liveEventTab.partial.vue";
    import Panel from "@Obsidian/Controls/panel";
    import PanelNavigationBar from "./ExperienceManager/panelNavigationBar.partial.vue";
    import { useConfigurationValues, useReloadBlock, onConfigurationValuesChanged } from "@Obsidian/Utility/block";
    import { ExperienceManagerInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/ExperienceManager/experienceManagerInitializationBox";
    import { computed, ref } from "vue";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { NavigationUrlKey } from "./ExperienceManager/types";

    const config = useConfigurationValues<ExperienceManagerInitializationBox>();

    // #region Values

    const selectedTab = ref<"Live Event" | "Moderation" | "Live Questions">("Live Event");
    const navigationTabs: ListItemBag[] = ["Live Event", "Moderation", "Live Questions"].map(s => ({ value: s, text: s }));

    // #endregion

    // #region Computed Values

    const panelTitle = computed((): string => {
        return `Experience Manager: ${config.experienceName}`;
    });

    const isLiveEventTab = computed((): boolean => {
        return selectedTab.value === "Live Event";
    });

    const isModerationTab = computed((): boolean => {
        return selectedTab.value === "Moderation";
    });

    const isLiveQuestionsTab = computed((): boolean => {
        return selectedTab.value === "Live Questions";
    });

    // #endregion

    // #region Functions

    // #endregion

    // #region Event Handlers

    // #endregion

    onConfigurationValuesChanged(useReloadBlock());
</script>
