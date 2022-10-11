<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <DateTimePicker v-model="startDateTime"
                    label="Start Date / Time"
                    help=""
                    :rules="requiredRules" />

    <RockFormField :modelValue="duration"
                   label="Duration"
                   name="duration"
                   :rules="requiredRules">
        <template #default>
            <div class="form-control-group">
                <NumberBox v-model="durationInHours"
                           inputGroupClasses="input-width-md"
                           :rules="requiredRules">
                    <template #append>
                        <span class="input-group-addon">hrs</span>
                    </template>
                </NumberBox>

                <NumberBox v-model="durationInMinutes"
                           inputGroupClasses="input-width-md"
                           :rules="requiredRules">
                    <template #append>
                        <span class="input-group-addon">mins</span>
                    </template>
                </NumberBox>
            </div>
        </template>
    </RockFormField>

    <RadioButtonList v-model="scheduleType"
                     :items="scheduleTypeItems"
                     horizontal />

    <TransitionVerticalCollapse>
        <div v-if="isRecurringSchedule">
            <legend class="legend-small">Recurrence</legend>

            <RadioButtonList v-model="occurrencePattern"
                             label="Occurrence Pattern"
                             :items="occurrencePatternItems"
                             horizontal />

            <div v-if="isRecurringSpecificDates"
                 class="recurrence-pattern-type control-group controls recurrence-pattern-specific-date">
                <ul>
                    <li v-for="date of specificDates">
                        <span>{{ getShortDateText(date) }}</span>&ThinSpace;
                        <a href="#" class="text-danger" @click.prevent="onRemoveSpecificDate(date)"><i class="fa fa-times"></i></a>
                    </li>
                </ul>

                <div v-if="isAddSpecificDateVisible">
                    <table>
                        <tr>
                            <td>
                                <DatePicker v-model="addSpecificDateValue" />
                            </td>
                            <td>
                                <a class="btn btn-primary btn-xs add-specific-date-ok ml-3"
                                   @click.prevent="onAddSpecificDateOk">
                                    <span>OK</span>
                                </a>
                                <a class="btn btn-link btn-xs add-specific-date-cancel"
                                   @click.prevent="onAddSpecificDateCancel">
                                    <span>Cancel</span>
                                </a>
                            </td>
                        </tr>
                    </table>
                </div>
                <a v-else class="btn btn-action btn-sm add-specific-date"
                   @click.prevent="onAddSpecificDate">
                    <i class="fa fa-plus"></i>
                    <span>&nbsp;Add Date</span>
                </a>
            </div>

            <div v-if="isRecurringDaily"
                 class="recurrence-pattern-type recurrence-pattern-daily">
                <div class="form-control-group">
                    <label class="radio-inline">
                        <input :id="dailyRadioId" :name="dailyRadioId" type="radio" value="everyDay" v-model="recurringDailyType" />
                        <span class="label-text">Every</span>
                    </label>
                    <NumberBox v-model="recurringDailyDays"
                               inputGroupClasses="input-width-md">
                        <template #append>
                            <span class="input-group-addon">days</span>
                        </template>
                    </NumberBox>
                </div>

                <div class="form-control-group">
                    <label class="radio-inline">
                        <input :id="dailyRadioId" :name="dailyRadioId" type="radio" value="everyWeekday" v-model="recurringDailyType" />
                        <span class="label-text">Every weekday</span>
                    </label>
                </div>

                <div class="form-control-group">
                    <label class="radio-inline">
                        <input :id="dailyRadioId" :name="dailyRadioId" type="radio" value="everyWeekend" v-model="recurringDailyType" />
                        <span class="label-text">Every weekend</span>
                    </label>
                </div>
            </div>

            <div v-if="isRecurringWeekly"
                 class="recurrence-pattern-type recurrence-pattern-weekly">
                <div class="form-control-group">
                    <span>Every&ThinSpace;</span>
                    <NumberBox v-model="recurringWeeklyWeeks"
                               inputGroupClasses="input-width-md">
                        <template #append>
                            <span class="input-group-addon">week(s)</span>
                        </template>
                    </NumberBox>
                    <span>&ThinSpace;on</span>
                </div>

                <div class="week-days">
                    <CheckBoxList v-model="recurringWeeklyDays"
                                  :items="recurringWeeklyDaysItems"
                                  horizontal />
                </div>
            </div>



            <div v-if="isRecurringUntil"
                 class="continue-until">
                <div class="controls">
                    <hr />
                </div>

                <label class="control-label">Continue Until</label>

                <div class="controls">
                    <div class="form-control-group">
                        <label class="radio-inline">
                            <input :id="continueUntilRadioId" :name="continueUntilRadioId" type="radio" value="noEnd" v-model="recurringContinueUntilType" />
                            <span class="label-text">No end</span>
                        </label>
                    </div>

                    <div class="form-control-group">
                        <label class="radio-inline">
                            <input :id="continueUntilRadioId" :name="continueUntilRadioId" type="radio" value="endBy" v-model="recurringContinueUntilType" />
                            <span class="label-text">End by</span>
                        </label>

                        <DatePickerBase v-model="recurringContinueUntilDate" />
                    </div>

                    <div class="form-control-group">
                        <label class="radio-inline">
                            <input :id="continueUntilRadioId" :name="continueUntilRadioId" type="radio" value="endAfter" v-model="recurringContinueUntilType" />
                            <span class="label-text">End after</span>
                        </label>

                        <NumberBox v-model="recurringContinueUntilCount"
                                   inputGroupClasses="input-width-lg">
                            <template #append>
                                <span class="input-group-addon">occurrences</span>
                            </template>
                        </NumberBox>
                    </div>
                </div>
            </div>
        </div>
    </TransitionVerticalCollapse>
</template>

<style scoped>
.recurrence-pattern-specific-date>ul>li>a {
    display: none;
}

.recurrence-pattern-specific-date>ul:hover>li>a {
    display: initial;
}
</style>

<script setup lang="ts">
    import { PropType, computed, ref, shallowRef } from "vue";
    import CheckBoxList from "./checkBoxList";
    import DatePicker, { DatePickerBase } from "./datePicker";
    import DateRangePicker from "./dateRangePicker";
    import DateTimePicker from "./dateTimePicker";
    import NumberBox from "./numberBox";
    import RadioButtonList from "./radioButtonList";
    import RockFormField from "./rockFormField";
    import TransitionVerticalCollapse from "./transitionVerticalCollapse";
    import type { ValidationRule } from "@Obsidian/Types/validationRules";
    import { Event, Calendar } from "@Obsidian/Utility/internetCalendar";
    import { rulesPropType, containsRequiredRule } from "@Obsidian/ValidationRules";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
    import { updateRefValue } from "@Obsidian/Utility/component";
    import { newGuid } from "@Obsidian/Utility/guid";

    const props = defineProps({
        modelValue: {
            type: String as PropType<string>,
            default: ""
        },

        rules: rulesPropType
    });

    const emit = defineEmits<{
        (e: "update:modelValue", value: string): void
    }>();

    // #region Values

    const dailyRadioId = newGuid().toString();
    const continueUntilRadioId = newGuid().toString();
    const event = getEventFromCalendar(props.modelValue);

    const startDateTime = ref(event?.startDateTime?.toISOString() ?? null);
    const duration = ref(getEventDuration(event));
    const scheduleType = ref(event?.recurrenceRules && event.recurrenceRules.length > 0 ? "recurring" : "oneTime");
    const occurrencePattern = ref(getEventOccurrencePattern(event));

    // A shallowRef behaves slightly differently than ref, but it the same for
    // our use case and makes some things happy in the template.
    const specificDates = shallowRef<RockDateTime[]>([]);

    const isAddSpecificDateVisible = ref(false);
    const addSpecificDateValue = ref("");
    const recurringDailyType = ref<"everyDay" | "everyWeekday" | "everyWeekend">("everyDay");
    const recurringDailyDays = ref(1);
    const recurringContinueUntilType = ref<"noEnd" | "endBy" | "endAfter">("noEnd");
    const recurringContinueUntilDate = ref("");
    const recurringContinueUntilCount = ref<number | null>(null);
    const recurringWeeklyWeeks = ref<number | null>(null);
    const recurringWeeklyDays = ref<("Sun" | "Mon" | "Tue" | "Wed" | "Thu" | "Fri" | "Sat")[]>([]);

    const scheduleTypeItems: ListItemBag[] = [
        {
            value: "oneTime",
            text: "One Time"
        },
        {
            value: "recurring",
            text: "Recurring"
        }
    ];

    const occurrencePatternItems: ListItemBag[] = [
        {
            value: "specificDates",
            text: "Specific Dates"
        },
        {
            value: "daily",
            text: "Daily"
        },
        {
            value: "weekly",
            text: "Weekly"
        },
        {
            value: "monthly",
            text: "Monthly"
        }
    ];

    const recurringWeeklyDaysItems: ListItemBag[] = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"].map(d => ({
        value: d,
        text: d
    }));

    // #endregion

    // #region Computed Values

    const requiredRules = computed((): ValidationRule[] => {
        if (containsRequiredRule(props.rules)) {
            return ["required"];
        }
        else {
            return [];
        }
    });

    const durationInHours = computed({
        get(): number | null {
            return duration.value ? Math.floor(duration.value / 60) : null;
        },

        set(value: number | null) {
            const newDuration = ((value ?? 0) * 60) + (durationInMinutes.value ?? 0);

            if (newDuration <= 0) {
                duration.value = null;
            }
            else {
                duration.value = newDuration;
            }
        }
    });

    const durationInMinutes = computed({
        get(): number | null {
            return duration.value ? Math.floor(duration.value % 60) : null;
        },

        set(value: number | null) {
            const newDuration = ((durationInHours.value ?? 0) * 60) + (value ?? 0);

            if (newDuration <= 0) {
                duration.value = null;
            }
            else {
                duration.value = newDuration;
            }
        }
    });

    const isRecurringSchedule = computed((): boolean => {
        return scheduleType.value == "recurring";
    });

    const isRecurringSpecificDates = computed((): boolean => {
        return occurrencePattern.value === "specificDates";
    });

    const isRecurringDaily = computed((): boolean => {
        return occurrencePattern.value === "daily";
    });

    const isRecurringWeekly = computed((): boolean => {
        return occurrencePattern.value === "weekly";
    });

    const isRecurringMonthly = computed((): boolean => {
        return occurrencePattern.value === "monthly";
    });

    const isRecurringUntil = computed((): boolean => {
        return isRecurringDaily.value || isRecurringWeekly.value || isRecurringMonthly.value;
    });

    // #endregion

    // #region Functions

    function getEventFromCalendar(ical: string): Event | null {
        if (!ical) {
            return null;
        }

        try {
            const calendar = new Calendar(ical);

            return calendar.events.length > 0 ? calendar.events[0] : null;
        }
        catch {
            return null;
        }
    }

    function getEventDuration(event: Event | null): number | null {
        if (!event || !event.startDateTime || !event.endDateTime) {
            return null;
        }

        const totalDurationInMinutes = (event.endDateTime.toMilliseconds() - event.startDateTime.toMilliseconds()) / 1000 / 60;

        if (totalDurationInMinutes <= 0) {
            return null;
        }

        return Math.floor(totalDurationInMinutes);
    }

    function getEventOccurrencePattern(event: Event | null): "specificDates" | "daily" | "weekly" | "monthly" {
        if (!event || !event.recurrenceRules.length) {
            return "specificDates";
        }

        const rrule = event.recurrenceRules[0];

        if (rrule.frequency === "DAILY") {
            return "daily";
        }
        else if (rrule.frequency === "WEEKLY") {
            return "weekly";
        }
        else if (rrule.frequency === "MONTHLY") {
            return "monthly";
        }
        else {
            return "specificDates";
        }
    }

    function getShortDateText(date: RockDateTime): string {
        return date.toLocaleString({
            year: "numeric",
            month: "2-digit",
            day: "2-digit"
        });
    }

    // #endregion

    // #region Event Handlers

    function onAddSpecificDate(): void {
        addSpecificDateValue.value = "";
        isAddSpecificDateVisible.value = true;
    }

    function onRemoveSpecificDate(date: RockDateTime): void {
        console.log("remove", date, specificDates.value);
        var newSpecificDates = specificDates.value.filter(d => d.toMilliseconds() !== date.toMilliseconds());

        updateRefValue(specificDates, newSpecificDates);
    }

    function onAddSpecificDateOk(): void {
        const date = RockDateTime.parseISO(addSpecificDateValue.value);

        if (date !== null && !specificDates.value.some(d => d.toMilliseconds() === date.toMilliseconds())) {
            const newSpecificDates = [date, ...specificDates.value];

            newSpecificDates.sort((a, b) => a.toMilliseconds() - b.toMilliseconds());

            updateRefValue(specificDates, newSpecificDates);
        }
        isAddSpecificDateVisible.value = false;
    }

    function onAddSpecificDateCancel(): void {
        isAddSpecificDateVisible.value = false;
    }

    // #endregion

</script>