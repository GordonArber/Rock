// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { DayOfWeek, RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { newGuid } from "@Obsidian/Utility/guid";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { pluralConditional } from "@Obsidian/Utility/stringUtils";

function padZeroLeft(value: number, length: number): string {
    const str = value.toString();

    return "0".repeat(length - str.length) + str;
}

type Frequency = "DAILY" | "WEEKLY" | "MONTHLY";

type Weekday = "SU" | "MO" | "TU" | "WE" | "TH" | "FR" | "SA";

type WeekdayNumber = {
    value: number;

    day: Weekday;
};

function getDateString(date: Date): string {
    const year = date.getFullYear();
    const month = date.getMonth() + 1;
    const day = date.getDate();

    return `${year}${padZeroLeft(month, 2)}${padZeroLeft(day, 2)}`;
}

function getTimeString(date: Date): string {
    const hour = date.getHours();
    const minute = date.getMinutes();
    const second = date.getSeconds();

    return `${padZeroLeft(hour, 2)}${padZeroLeft(minute, 2)}${padZeroLeft(second, 2)}`;
}

function getDateTimeString(date: Date): string {
    return `${getDateString(date)}T${getTimeString(date)}`;
}

function getDatesFromRangeOrPeriod(value: string): Date[] {
    const segments = value.split("/");

    if (segments.length === 0) {
        return [];
    }

    const startDate = getDateFromString(segments[0]);
    if (!startDate) {
        return [];
    }

    if (segments.length !== 2) {
        return [startDate];
    }

    const dates: Date[] = [];

    if (segments[1].startsWith("P")) {
        const days = getPeriodDurationInDays(segments[1]);

        for (let day = 0; day < days; day++) {
            const date = new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate() + day);
            dates.push(date);
        }
    }
    else {
        const endDate = getDateFromString(segments[1]);

        if (endDate !== null) {
            let date = startDate;

            while (date <= endDate) {
                dates.push(date);
                date = new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1);
            }
        }
    }

    return dates;
}

function getDateFromString(value: string): Date | null {
    if (value.length < 8) {
        return null;
    }

    const year = parseInt(value.substring(0, 4));
    const month = parseInt(value.substring(4, 6)) - 1;
    const day = parseInt(value.substring(6, 8));

    return new Date(year, month, day);
}

function getDateTimeFromString(value: string): Date | null {
    if (value.length < 15 || value[8] !== "T") {
        return null;
    }

    const year = parseInt(value.substring(0, 4));
    const month = parseInt(value.substring(4, 6)) - 1;
    const day = parseInt(value.substring(6, 8));
    const hour = parseInt(value.substring(9, 11));
    const minute = parseInt(value.substring(11, 13));
    const second = parseInt(value.substring(13, 15));

    return new Date(year, month, day, hour, minute, second);
}

function getPeriodDurationInDays(period: string): number {
    if (!period.startsWith("P")) {
        return 0;
    }

    if (period.endsWith("D")) {
        return parseInt(period.substring(1, period.length - 1));
    }
    else if (period.endsWith("W")) {
        return parseInt(period.substring(1, period.length - 1)) * 7;
    }

    return 0;
}

function getRecurrenceDates(attributes: Record<string, string>, value: string): Date[] {
    const recurrenceDates: Date[] = [];
    const valueParts = value.split(",");
    const valueType = attributes["VALUE"];

    for (const valuePart of valueParts) {
        if (valueType === "PERIOD") {
            recurrenceDates.push(...getDatesFromRangeOrPeriod(valuePart));
        }
        else if (valueType === "DATE") {
            const date = getDateFromString(valuePart);
            if (date) {
                recurrenceDates.push(date);
            }
        }
        else {
            const date = getDateTimeFromString(valuePart);
            if (date) {
                recurrenceDates.push(date);
            }
        }
    }

    return recurrenceDates;
}

function getWeekdayName(day: Weekday): string {
    if (day === "SU") {
        return "Sunday";
    }
    else if (day === "MO") {
        return "Monday";
    }
    else if (day === "TU") {
        return "Tuesday";
    }
    else if (day === "WE") {
        return "Wednesday";
    }
    else if (day === "TH") {
        return "Thursday";
    }
    else if (day === "FR") {
        return "Friday";
    }
    else if (day === "SA") {
        return "Saturday";
    }
    else {
        return "Unknown";
    }
}

function dateMatchesDays(rockDate: RockDateTime, days: Weekday[]): boolean {
    for (const day of days) {
        if (rockDate.dayOfWeek === DayOfWeek.Sunday && day === "SU") {
            return true;
        }
        else if (rockDate.dayOfWeek === DayOfWeek.Monday && day === "MO") {
            return true;
        }
        else if (rockDate.dayOfWeek === DayOfWeek.Tuesday && day === "TU") {
            return true;
        }
        else if (rockDate.dayOfWeek === DayOfWeek.Wednesday && day === "WE") {
            return true;
        }
        else if (rockDate.dayOfWeek === DayOfWeek.Thursday && day === "TH") {
            return true;
        }
        else if (rockDate.dayOfWeek === DayOfWeek.Friday && day === "FR") {
            return true;
        }
        else if (rockDate.dayOfWeek === DayOfWeek.Saturday && day === "SA") {
            return true;
        }
    }

    return false;
}

function dateMatchesOffsetDayOfWeeks(rockDate: RockDateTime, dayOfWeek: Weekday, offsets: number[]): boolean {
    if (!dateMatchesDays(rockDate, [dayOfWeek])) {
        return false;
    }

    const dayOfMonth = rockDate.day;

    for (const offset of offsets) {
        if (offset === 1 && dayOfMonth >= 1 && dayOfMonth <= 7) {
            return true;
        }
        else if (offset === 2 && dayOfMonth >= 8 && dayOfMonth <= 14) {
            return true;
        }
        else if (offset === 3 && dayOfMonth >= 15 && dayOfMonth <= 21) {
            return true;
        }
        else if (offset === 4 && dayOfMonth >= 22 && dayOfMonth <= 28) {
            return true;
        }
        else if (offset === -1) {
            const lastDayOfMonth = rockDate.addDays(-(rockDate.day - 1)).addMonths(1).addDays(-1).day;

            console.log(lastDayOfMonth, dayOfMonth);

            if (dayOfMonth >= (lastDayOfMonth - 7) && dayOfMonth <= lastDayOfMonth) {
                console.log("Day matches", rockDate.toString());
                return true;
            }
        }
    }

    return false;
}

export class RecurrenceRuleBuilder {
    public frequency?: Frequency;

    public endDate?: Date;

    public count?: number;

    public interval: number = 1;

    public byMonthDay: number[] = [];

    public byDay: WeekdayNumber[] = [];

    public constructor(rule: string | undefined = undefined) {
        if (!rule) {
            return;
        }

        const values: Record<string, string> = {};

        for (const attr of rule.split(";")) {
            const attrParts = attr.split("=");
            if (attrParts.length === 2) {
                values[attrParts[0]] = attrParts[1];
            }
        }

        if (values["UNTIL"] !== undefined && values["COUNT"] !== undefined) {
            throw new Error(`Recurrence rule '${rule}' cannot specify both UNTIL and COUNT.`);
        }

        if (values["FREQ"] !== "DAILY" && values["FREQ"] !== "WEEKLY" && values["FREQ"] !== "MONTHLY") {
            throw new Error(`Invalid frequence for recurrence rule '${rule}'.`);
        }

        this.frequency = values["FREQ"];

        if (values["UNTIL"]?.length === 8) {
            this.endDate = getDateFromString(values["UNTIL"]) ?? undefined;
        }
        else if (values["UNTIL"]?.length >= 15) {
            this.endDate = getDateTimeFromString(values["UNTIL"]) ?? undefined;
        }

        if (values["COUNT"] !== undefined) {
            this.count = toNumberOrNull(values["COUNT"]) ?? undefined;
        }

        if (values["INTERVAL"] !== undefined) {
            this.interval = toNumberOrNull(values["INTERVAL"]) ?? 1;
        }

        if (values["BYMONTHDAY"] !== undefined && values["BYMONTHDAY"].length > 0) {
            this.byMonthDay = [];

            for (const v of values["BYMONTHDAY"].split(",")) {
                const num = toNumberOrNull(v);
                if (num !== null) {
                    this.byMonthDay.push(num);
                }
            }
        }

        if (values["BYDAY"] !== undefined && values["BYDAY"].length > 0) {
            this.byDay = [];

            for (const v of values["BYDAY"].split(",")) {
                if (v.length < 2) {
                    continue;
                }

                const num = v.length > 2 ? toNumberOrNull(v.substring(0, v.length - 2)) : 1;
                const day = v.substring(v.length - 2);

                if (num === null) {
                    continue;
                }

                if (day === "SU" || day === "MO" || day === "TU" || day == "WE" || day == "TH" || day == "FR" || day == "SA") {
                    this.byDay.push({
                        value: num,
                        day: day
                    });
                }
            }
        }
    }

    public build(): string {
        const attributes: string[] = [];

        attributes.push(`FREQ=${this.frequency}`);

        if (this.count !== undefined) {
            attributes.push(`COUNT=${this.count}`);
        }
        else if (this.endDate) {
            attributes.push(`UNTIL=${getDateTimeString(this.endDate)}`);
        }

        if (this.interval > 1) {
            attributes.push(`INTERVAL=${this.interval}`);
        }

        if (this.byMonthDay.length > 0) {
            const monthDayValues = this.byMonthDay.map(md => md.toString()).join(",");
            attributes.push(`BYMONTHDAY=${monthDayValues}`);
        }

        if (this.byDay.length > 0) {
            const dayValues = this.byDay.map(d => d.value > 1 ? `${d.value}${d.day}` : d.day);
            attributes.push(`BYDAY=${dayValues}`);
        }

        return attributes.join(";");
    }

    public getDates(eventStartDateTime: RockDateTime, startDateTime: RockDateTime, endDateTime: RockDateTime): RockDateTime[] {
        const dates: RockDateTime[] = [];
        let rockDate = eventStartDateTime;
        let dateCount = 0;

        if (!rockDate) {
            return [];
        }

        if (this.endDate) {
            const ruleEndDate = RockDateTime.fromJSDate(this.endDate);

            if (ruleEndDate && ruleEndDate < endDateTime) {
                endDateTime = ruleEndDate;
            }
        }

        while (rockDate < endDateTime) {
            if (this.count && dateCount >= this.count) {
                break;
            }

            dateCount++;

            if (rockDate >= startDateTime) {
                dates.push(rockDate);
            }

            const nextDate = this.nextDateAfter(rockDate);

            if (nextDate === null) {
                break;
            }
            else {
                rockDate = nextDate;
            }
        }

        return dates;
    }

    private nextDateAfter(rockDate: RockDateTime): RockDateTime | null {
        if (this.frequency === "DAILY") {
            return rockDate.addDays(this.interval);
        }
        else if (this.frequency === "WEEKLY" && this.byDay.length > 0) {
            let nextDate = rockDate;

            if (nextDate.dayOfWeek === DayOfWeek.Saturday) {
                nextDate = nextDate.addDays(1 + ((this.interval - 1) * 7));
            }
            else {
                nextDate = nextDate.addDays(1);
            }

            while (!dateMatchesDays(nextDate, this.byDay.map(d => d.day))) {
                if (nextDate.dayOfWeek === DayOfWeek.Saturday) {
                    nextDate = nextDate.addDays(1 + ((this.interval - 1) * 7));
                }
                else {
                    nextDate = nextDate.addDays(1);
                }
            }

            return nextDate;
        }
        else if (this.frequency === "MONTHLY") {
            if (this.byMonthDay.length > 0) {
                let nextDate = rockDate.addDays(-(rockDate.day - 1));

                if (rockDate.day >= this.byMonthDay[0]) {
                    nextDate = nextDate.addMonths(this.interval);
                }

                let lastDayOfMonth = nextDate.addMonths(1).addDays(-1).day;
                let loopCount = 0;

                // Skip any months that don't have this day number.
                while (lastDayOfMonth < this.byMonthDay[0]) {
                    nextDate = nextDate.addMonths(this.interval);

                    lastDayOfMonth = nextDate.addMonths(1).addDays(-1).day;
                    if (loopCount++ >= 100) {
                        return null;
                    }
                }

                nextDate = nextDate.addDays(this.byMonthDay[0] - 1);

                return nextDate;
            }
            else if (this.byDay.length > 0) {
                const dayOfWeek = this.byDay[0].day;
                const offsets = this.byDay.map(d => d.value);

                let nextDate = rockDate.addDays(1);

                while (!dateMatchesOffsetDayOfWeeks(nextDate, dayOfWeek, offsets)) {
                    nextDate = nextDate.addDays(1);
                }

                return nextDate;
            }
        }

        return null;
    }
}

export class ScheduleRuleBuilder {
    public startDateTime?: Date;

    public endDateTime?: Date;

    public excludedDates: Date[] = [];

    public recurrenceDates: Date[] = [];

    public recurrenceRules: RecurrenceRuleBuilder[] = [];

    public uid?: string;

    public constructor(icalContent: string | undefined = undefined) {
        if (icalContent !== undefined) {
            this.parse(icalContent);
        }
        else {
            this.uid = newGuid();
        }
    }

    public build(): string | null {
        const lines: string[] = [];

        if (!this.startDateTime || !this.endDateTime) {
            return null;
        }

        lines.push("BEGIN:VCALENDAR");
        lines.push("PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN");
        lines.push("VERSION:2.0");
        lines.push("BEGIN:VEVENT");
        lines.push(`DTEND:${getDateTimeString(this.endDateTime)}`);
        lines.push(`DTSTAMP:${getDateTimeString(new Date())}`);
        lines.push(`DTSTART:${getDateTimeString(this.startDateTime)}`);

        if (this.excludedDates.length > 0) {
            lines.push(`EXDATE:${this.excludedDates.map(d => getDateString(d) + "/P1D").join(",")}`);
        }

        if (this.recurrenceDates.length > 0) {
            const recurrenceDates: string[] = [];
            for (const date of this.recurrenceDates) {
                const rDate = new Date(date.getFullYear(), date.getMonth(), date.getDate(), this.startDateTime.getHours(), this.startDateTime.getMinutes(), this.startDateTime.getSeconds());
                recurrenceDates.push(getDateTimeString(rDate));
            }

            lines.push(`RDATE:${recurrenceDates.join(",")}`);
        }
        else if (this.recurrenceRules.length > 0) {
            for (const rrule of this.recurrenceRules) {
                lines.push(`RRULE:${rrule.build()}`);
            }
        }

        lines.push("SEQUENCE:0");
        lines.push(`UID:${this.uid}`);
        lines.push("END:VEVENT");
        lines.push("END:VCALENDAR");

        for (let lineNumber = 0; lineNumber < lines.length; lineNumber++) {
            // Spec does not allow lines longer than 75 characters.
            if (lines[lineNumber].length > 75) {
                const currentLine = lines[lineNumber].substring(0, 75);
                const newLine = " " + lines[lineNumber].substring(75);

                lines.splice(lineNumber, 1, currentLine, newLine);
            }
        }

        return lines.join("\r\n");
    }

    private parse(iCalContent: string): void {
        const lines = iCalContent.split(/\r\n|\n|\r/);
        let blockType: string | null = null;
        let duration: string | null = null;

        // Convert all continuation lines into single lines.
        for (let lineNumber = 1; lineNumber < lines.length;) {
            if (lines[lineNumber][0] === " ") {
                lines[lineNumber - 1] += lines[lineNumber].substring(1);
                lines.splice(lineNumber, 1);
            }
            else {
                lineNumber++;
            }
        }

        // Parse the line data.
        for (const line of lines) {
            if (line === "BEGIN:VEVENT") {
                blockType = "VEVENT";
            }

            if (blockType === null) {
                continue;
            }

            if (line === "END:VEVENT") {
                break;
            }

            const splitAt = line.indexOf(":");
            if (splitAt < 0) {
                continue;
            }

            let key = line.substring(0, splitAt);
            const value = line.substring(splitAt + 1);

            const keyAttributes: Record<string, string> = {};
            const keySegments = key.split(";");
            if (keySegments.length > 1) {
                key = keySegments[0];
                keySegments.splice(0, 1);

                for (const attr of keySegments) {
                    const attrSegments = attr.split("=");
                    if (attr.length === 2) {
                        keyAttributes[attrSegments[0]] = attrSegments[1];
                    }
                }
            }

            if (key === "DTSTART") {
                this.startDateTime = getDateTimeFromString(value) ?? undefined;
            }
            else if (key === "DTEND") {
                this.endDateTime = getDateTimeFromString(value) ?? undefined;
            }
            else if (key === "RRULE") {
                this.recurrenceRules.push(new RecurrenceRuleBuilder(value));
            }
            else if (key === "RDATE") {
                this.recurrenceDates = getRecurrenceDates(keyAttributes, value);
            }
            else if (key === "UID") {
                this.uid = value;
            }
            else if (key === "DURATION") {
                duration = value;
            }
            else if (key === "EXDATE") {
                const dateValues = value.split(",");
                for (const dateValue of dateValues) {
                    const dates = getDatesFromRangeOrPeriod(dateValue);
                    this.excludedDates.push(...dates);
                }
            }
        }

        if (duration !== null) {
            // TODO: Calculate number of seconds and add to startDate.
        }
    }

    private isDateExcluded(rockDate: RockDateTime): boolean {
        const rockDateOnly = rockDate.date;

        for (const excludedDate of this.excludedDates) {
            const rockExcludedDate = RockDateTime.fromJSDate(excludedDate);

            if (rockExcludedDate && rockExcludedDate.date.isEqualTo(rockDateOnly)) {
                return true;
            }
        }

        return false;
    }

    public getDates(startDateTime: RockDateTime, endDateTime: RockDateTime): RockDateTime[] {
        const eventStartDateTime = this.startDateTime ? RockDateTime.fromJSDate(this.startDateTime) : null;
        if (!this.startDateTime || !eventStartDateTime) {
            return [];
        }

        // If the schedule has a startDateTime that is later than the requested
        // startDateTime then use ours instead.
        if (this.startDateTime) {
            const rockDate = RockDateTime.fromJSDate(this.startDateTime);

            if (rockDate && rockDate > startDateTime) {
                startDateTime = rockDate;
            }
        }

        if (this.recurrenceDates.length > 0) {
            const dates: RockDateTime[] = [];
            const recurrenceDates: Date[] = [this.startDateTime, ...this.recurrenceDates];

            for (const d of recurrenceDates) {
                const rockDate = RockDateTime.fromJSDate(d);

                if (rockDate && rockDate >= startDateTime && rockDate < endDateTime) {
                    dates.push(rockDate);
                }
            }

            return dates;
        }
        else if (this.recurrenceRules.length > 0) {
            const rrule = this.recurrenceRules[0];

            return rrule.getDates(eventStartDateTime, startDateTime, endDateTime)
                .filter(d => !this.isDateExcluded(d));
        }
        else {
            if (eventStartDateTime && eventStartDateTime >= startDateTime && eventStartDateTime < endDateTime) {
                return [eventStartDateTime];
            }

            return [];
        }
    }

    public toFriendlyText(): string {
        return this.toFriendlyFormat(false);
    }

    public toFriendlyHtml(): string {
        return this.toFriendlyFormat(true);
    }

    private toFriendlyFormat(html: boolean): string {
        if (!this.startDateTime) {
            return "";
        }

        const startTimeText = RockDateTime.fromJSDate(this.startDateTime)?.toLocaleString({ hour: "numeric", minute: "2-digit", hour12: true });

        if (this.recurrenceRules.length > 0) {
            const rrule = this.recurrenceRules[0];

            if (rrule.frequency === "DAILY") {
                let result = "Daily";

                if (rrule.interval > 1) {
                    result += ` every ${rrule.interval} ${pluralConditional(rrule.interval, "day", "days")}`;
                }

                result += ` at ${startTimeText}`;

                return result;
            }
            else if (rrule.frequency === "WEEKLY") {
                if (rrule.byDay.length === 0) {
                    return "No Scheduled Days";
                }

                let result = rrule.byDay.map(d => getWeekdayName(d.day) + "s").join(",");

                if (rrule.interval > 1) {
                    result = `Every ${rrule.interval} weeks: ${result}`;
                }
                else {
                    result = `Weekly: ${result}`;
                }

                return `${result} at ${startTimeText}`;
            }
            else if (rrule.frequency === "MONTHLY") {
                if (rrule.byMonthDay.length > 0) {
                    let result = `Day ${rrule.byMonthDay[0]} of every `;

                    if (rrule.interval > 1) {
                        result += `${rrule.interval} months`;
                    }
                    else {
                        result += "month";
                    }

                    return `${result} at ${startTimeText}`;
                }
                else if (rrule.byDay.length > 0) {
                    const byDay = rrule.byDay[0];
                    const offsetNames = nthNamesAbbreviated.filter(n => rrule.byDay.some(d => d.value == n[0])).map(n => n[1]);
                    let result = "";

                    if (offsetNames.length > 0) {
                        let nameText;

                        if (offsetNames.length > 2) {
                            nameText = `${offsetNames.slice(0, offsetNames.length - 1).join(", ")} and ${offsetNames[offsetNames.length - 1]}`;
                        }
                        else {
                            nameText = offsetNames.join(" and ");
                        }
                        result = `The ${nameText} ${getWeekdayName(byDay.day)} of every month`;
                    }
                    else {
                        return "";
                    }

                    return `${result} at ${startTimeText}`;
                }
                else {
                    return "";
                }
            }
            else {
                return "";
            }
        }
        else {
            const dates: Date[] = [this.startDateTime, ...this.recurrenceDates];

            if (!html || dates.length > 99) {
                const firstDate = RockDateTime.fromJSDate(dates[0]);
                const lastDate = RockDateTime.fromJSDate(dates[dates.length - 1]);

                if (firstDate && lastDate) {
                    return `Multiple dates between ${firstDate.toASPString("g")} and ${lastDate.toASPString("g")}`;
                }
                else {
                    return "";
                }
            }
            else if (dates.length > 1) {
                let listHtml = `<ul class="list-unstyled">`;

                for (const date of dates) {
                    const rockDate = RockDateTime.fromJSDate(date);

                    if (rockDate) {
                        listHtml += `<li>${rockDate.toASPString("g")}</li>`;
                    }
                }

                listHtml += "</li>";

                return listHtml;
            }
            else if (dates.length === 1) {
                const rockDate = RockDateTime.fromJSDate(this.startDateTime);

                if (!rockDate) {
                    return "";
                }

                return `Once at ${rockDate.toASPString("g")}`;
            }
            else {
                return "No Schedule";
            }
        }
    }
}

const nthNamesAbbreviated: [number, string][] = [
    [1, "1st"],
    [2, "2nd"],
    [3, "3rd"],
    [4, "4th"],
    [-1, "last"]
];
