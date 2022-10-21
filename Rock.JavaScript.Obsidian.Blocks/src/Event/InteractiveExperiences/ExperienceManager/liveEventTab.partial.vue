<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div class="row">
        <div class="col-lg-3 col-md-4 col-sm-6">
            <Kpi class="ml-0"
                 color="blue"
                 :colorShade="600"
                 :value="1234"
                 label="Current Individuals"
                 iconCssClass="fa fa-user"
                 isCard
                 tooltip="The number of individuals that are currently participating in the experience." />
        </div>
    </div>

    <div class="experience-body">
        <div class="experience-actions-panel">
            <div class="experience-actions-panel-header">
                <span class="title">Experience Actions</span>
                <a ref="notificationStateElement" href="#" :class="notificationStateClass" @click.prevent="onNotificationStateClick">
                    <i :class="notificationStateIconClass"></i>
                </a>
            </div>

            <div class="experience-actions-panel-body">
                <ExperienceActionButtons :actions="experienceActions" />
            </div>
        </div>

        <div class="preview-panel">
            This is a preview.
        </div>
    </div>
    hello!
</template>

<style scoped>
.experience-body {
    display: flex;
    margin: 0px -18px;
}

.experience-actions-panel {
    flex-grow: 1;
    min-width: 320px;
    min-height: 480px;
    margin: 0px 18px 18px 18px;
    background-color: var(--panel-heading-bg);
    border: 1px solid #c4c4c4;
    border-radius: var(--border-radius-base);
}

.experience-actions-panel-header {
    display: flex;
    padding: 8px;
    border-bottom: 1px solid #c4c4c4;
    align-items: center;
}

.experience-actions-panel-header > .title {
    flex-grow: 1;
}

.experience-actions-panel-body {
    padding: 12px;
}

.preview-panel {
    flex-grow: 1;
    min-width: 320px;
    max-width: 640px;
    min-height: 480px;
    margin: 0px 18px 18px 18px;
    border: 1px solid #c4c4c4;
    border-radius: var(--border-radius-base);
}
</style>

<script setup lang="ts">
    import Kpi from "@Obsidian/Controls/kpi.vue";
    import ExperienceActionButtons from "./experienceActionButtons.partial.vue";
    import { computed, PropType, ref } from "vue";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

    const props = defineProps({
    });

    const emit = defineEmits<{
    }>();

    // #region Values

    const notificationStateElement = ref<HTMLElement | null>(null);
    const isNotificationsEnabled = ref(false);

    const experienceActions: Record<string, string>[] = [
        {
            title: "First Item",
            iconCssClass: "fa fa-calendar"
        },
        {
            title: "Second Item",
            iconCssClass: "fa fa-user"
        }
    ];

    // #endregion

    // #region Computed Values

    const notificationStateClass = computed((): string => {
        return isNotificationsEnabled.value ? "btn btn-info btn-xs" : "btn btn-default btn-xs";
    });

    const notificationStateIconClass = computed((): string => {
        return isNotificationsEnabled.value ? "fa fa-fw fa-bell" : "fa fa-fw fa-bell-slash";
    });

    // #endregion

    // #region Functions

    // #endregion

    // #region Event Handlers

    /**
     * Event handler for when the notification bell is clicked. Toggle the
     * flag which determines if we are sending push notifications.
     */
    function onNotificationStateClick(): void {
        isNotificationsEnabled.value = !isNotificationsEnabled.value;

        if (notificationStateElement.value) {
            notificationStateElement.value.blur();
        }
    }

    // #endregion
</script>
