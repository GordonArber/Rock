<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <Panel title="Actions">
        <SectionHeader :title="actionHeaderTitle"
                       :description="actionHeaderDescription">
            <template #actions>
                <a class="btn btn-default btn-sm" href="#" @click.prevent="onAddActionClick">
                    <i class="fa fa-plus"></i>
                </a>
            </template>
        </SectionHeader>

        <div v-drag-source="dragOptions"
             v-drag-target="dragOptions"
             class="actions-list">
            <div v-for="(action, index) in internalValue" class="action-item" :key="action.guid!">
                <div>{{ index + 1 }}</div>
                <div>
                    <span class="reorder-handle">
                        <i class="fa fa-bars"></i>
                    </span>
                </div>
                <div class="action-item-icon">
                    <span class="icon">
                        <i :class="getActionTypeIconClass(action)"></i>
                    </span>
                </div>
                <div class="action-item-content">
                    <div class="title text-lg">{{ action.title }}</div>
                    <div class="subtitle text-sm text-muted">Subtitle: {{ getActionTypeName(action) }}</div>
                </div>
                <div>
                    <a href="#" @click.prevent="onActionRemoveClick(action)">
                        <i class="fa fa-times"></i>
                    </a>
                </div>
            </div>
        </div>
    </Panel>
</template>

<style scoped>
.action-item {
    display: flex;
    align-items: stretch;
    margin-bottom: 12px;
}

.action-item > * {
    display: flex;
    align-items: center;
    align-self: stretch;
    padding: 8px 12px;
    border-top: 1px solid #c4c4c4;
    border-bottom: 1px solid #c4c4c4;
}

.action-item > *:first-child {
    border-left: 1px solid #c4c4c4;
    border-radius: 8px 0px 0px 8px;
    background-color: var(--brand-info);
    color: white;
    justify-content: center;
    padding: 8px 0px;
    min-width: 35px;
}

.action-item > *:last-child {
    border-right: 1px solid #c4c4c4;
    border-radius: 0px 8px 8px 0px;
}

.action-item > .action-item-icon {
    padding-left: 0px;
}

.action-item > .action-item-icon > .icon {
    background-color: var(--brand-info);
    color: white;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 35px;
    height: 35px;
    border-radius: 50%;
}

.action-item > .action-item-content {
    flex: 1 0;
    flex-direction: column;
    align-items: stretch;
    justify-content: center;
    padding-left: 0px;
}

.action-item .reorder-handle {
    cursor: grab;
}
</style>

<script setup lang="ts">
    import Panel from "@Obsidian/Controls/panel";
    import SectionHeader from "@Obsidian/Controls/sectionHeader";
    import { DragSource as vDragSource, DragTarget as vDragTarget, useDragReorder } from "@Obsidian/Directives/dragDrop";
    import { useVModelPassthrough } from "@Obsidian/Utility/component";
    import { areEqual } from "@Obsidian/Utility/guid";
    import { InteractiveExperienceActionBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceActionBag";
    import { InteractiveExperienceActionTypeBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceActionTypeBag";
    import { computed, PropType, ref } from "vue";

    const props = defineProps({
        /** An array of actions that are currently configured. */
        modelValue: {
            type: Array as PropType<InteractiveExperienceActionBag[]>,
            required: true
        },

        /** The name of the experience currently displayed. */
        name: {
            type: String as PropType<string>,
            required: true
        },

        /** The action types that are supported by the server. */
        actionTypes: {
            type: Array as PropType<InteractiveExperienceActionTypeBag[]>,
            default: []
        }
    });

    const emit = defineEmits<{
        (e: "update:modelValue", value: InteractiveExperienceActionBag): void
    }>();

    // #region Values

    const internalValue = ref<InteractiveExperienceActionBag[]>([
        {
            guid: "one",
            actionType: {
                value: "5ffe1f8f-5f0b-4b34-9c3f-1706d9093210",
                text: "Test123",
            },
            title: "Test",
            isModerationRequired: false,
            isMultipleSubmissionsAllowed: false,
            isResponseAnonymous: false
        },
        {
            guid: "two",
            isModerationRequired: false,
            isMultipleSubmissionsAllowed: false,
            isResponseAnonymous: false
        },
        {
            guid: "three",
            isModerationRequired: false,
            isMultipleSubmissionsAllowed: false,
            isResponseAnonymous: false
        }]); //useVModelPassthrough(props, "modelValue", emit);

    // #endregion

    // #region Computed Values

    const actionHeaderTitle = computed((): string => {
        return `Actions for ${props.name}`;
    });

    const actionHeaderDescription = computed((): string => {
        return `The actions below are configured for the ${props.name} experience.`;
    });

    // #endregion

    // #region Functions

    function getActionTypeName(action: InteractiveExperienceActionBag): string {
        return props.actionTypes.find(at => areEqual(at.guid, action.actionType?.value))?.name ?? "";
    }

    function getActionTypeIconClass(action: InteractiveExperienceActionBag): string {
        return props.actionTypes.find(at => areEqual(at.guid, action.actionType?.value))?.iconCssClass ?? "";
    }

    // #endregion

    // #region Event Handlers

    function onAddActionClick(): void {
        /* */
    }

    function onActionRemoveClick(action: InteractiveExperienceActionBag): void {
        /* */
    }

    // #endregion

    const dragOptions = useDragReorder(internalValue, () => {
        // Force an update.
        internalValue.value = [...internalValue.value];
    });
</script>
