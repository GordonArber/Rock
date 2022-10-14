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
    </Panel>
</template>

<script setup lang="ts">
    import Panel from "@Obsidian/Controls/panel";
    import SectionHeader from "@Obsidian/Controls/sectionHeader";
    import { InteractiveExperienceBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceBag";
    import { computed, PropType, ref } from "vue";

    const props = defineProps({
        modelValue: {
            type: Array as PropType<Record<string, unknown>[]>,
            required: true
        },

        name: {
            type: String as PropType<string>,
            required: true
        }
    });

    const actionHeaderTitle = computed((): string => {
        return `Actions for ${props.name}`;
    });

    const actionHeaderDescription = computed((): string => {
        return `The actions below are configured for the ${props.name} experience.`;
    });

    function onAddActionClick(): void {
        /* */
    }
</script>
