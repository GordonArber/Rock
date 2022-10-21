<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div v-for="(action, index) in actions" class="action-item" :class="getActionItemClass(index)"
         href="#"
         @click.prevent="onActionClick(index)">
        <div class="action-item-icon">
            <span>{{ index + 1 }}</span>
            <span class="icon">
                <i :class="action.iconCssClass"></i>
            </span>
        </div>

        <div class="action-item-content">
            {{ action.title }}
        </div>

        <div class="action-item-selected-icon">
            <span class="icon">
                <i :class="getActionSelectedIconClass(index)"></i>
            </span>
        </div>
    </div>
</template>

<style scoped>
.action-item {
    display: flex;
    align-items: stretch;
    margin-bottom: 12px;
    cursor: pointer;
}

.action-item > * {
    display: flex;
    align-items: center;
    align-self: stretch;
    padding: 12px 12px;
    color: var(--text-color);
    background-color: white;
    border-top: 1px solid #c4c4c4;
    border-bottom: 1px solid #c4c4c4;
    transition: background-color 0.25s ease-in-out, border-color 0.25s ease-in-out;
}

.action-item > *:first-child {
    border-left: 1px solid #c4c4c4;
    border-right: 1px solid #c4c4c4;
    border-radius: 8px 0px 0px 8px;
    justify-content: center;
    padding-left: 8px;
    padding-right: 8px;
    min-width: 64px;
}

.action-item > *:last-child {
    border-right: 1px solid #c4c4c4;
    border-radius: 0px 8px 8px 0px;
    padding-right: 16px;
}

.action-item > .action-item-icon,
.action-item > .action-item-selected-icon {
    color: #777777;
}

.action-item > .action-item-icon > .icon {
    margin-left: 8px;
}

.action-item > .action-item-content {
    flex: 1 0;
}

.action-item:hover > * {
    background-color: rgba(85, 150, 230, 0.1);
}

.action-item.selected > * {
    color: #0079b0;
    background-color: #d9f2fe;
    border-color: #009ce3;
}

.action-item.selected > .action-item-selected-icon {
    color: #009ce3;
}
</style>

<script setup lang="ts">
    import { useVModelPassthrough } from "@Obsidian/Utility/component";
    import { PropType } from "vue";

    const props = defineProps({
        modelValue: {
            type: Number as PropType<number>,
            required: false
        },

        actions: {
            type: Array as PropType<Record<string, string>[]>,
            default: []
        }
    });

    const emit = defineEmits<{
        (e: "update:modelValue", value: number | undefined): void
    }>();

    // #region Values

    const internalValue = useVModelPassthrough(props, "modelValue", emit);

    // #endregion

    // #region Computed Values

    // #endregion

    // #region Functions

    function getActionItemClass(index: number): string {
        return index === internalValue.value ? "selected" : "";
    }

    function getActionSelectedIconClass(index: number): string {
        return index === internalValue.value ? "fa fa-check-circle" : "fa fa-check-circle-o";
    }

    // #endregion

    // #region Event Handlers

    function onActionClick(index: number): void {
        if (internalValue.value === index) {
            internalValue.value = undefined;
        }
        else {
            internalValue.value = index;
        }
    }

    // #endregion
</script>
