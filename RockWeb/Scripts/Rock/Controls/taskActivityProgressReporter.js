(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.taskActivityProgressReporter = (function () {
        const TaskActivityProgressReporter = function (options) {
            if (!Rock.RealTime) {
                throw new Error("realtime.js must be included first.");
            }

            if (!options.controlId) {
                throw new Error("controlId is required.");
            }

            this.controlId = options.controlId;
            this.connectionId = undefined;

            this.connect();
        }

        TaskActivityProgressReporter.prototype.getTaskId = function () {
            const control = document.getElementById(this.controlId);

            if (!control) {
                return null;
            }

            return control.getAttribute("data-task-id");
        };

        TaskActivityProgressReporter.prototype.connect = async function () {
            this.topic = await Rock.RealTime.getTopic("Rock.RealTime.Topics.TaskActivityProgressTopic");

            this.topic.on("TaskStarted", this.onTaskStarted.bind(this));
            this.topic.on("TaskCompleted", this.onTaskCompleted.bind(this));
            this.topic.on("UpdateTaskProgress", this.onUpdateTaskProgress.bind(this));

            const $connectionId = $(`#${this.controlId} [id$='_hfConnectionId']`);

            $connectionId.val(this.topic.connectionId || "");
        };

        TaskActivityProgressReporter.prototype.onTaskStarted = function (status) {
            // Intentionally left blank for now.
        };

        TaskActivityProgressReporter.prototype.onTaskCompleted = function (status) {
            const taskId = this.getTaskId();

            if (status.TaskId !== taskId) {
                return;
            }

            const $preparing = $(`#${this.controlId} .js-preparing`);
            const $progress = $(`#${this.controlId} .js-progress-div`);
            const $results = $(`#${this.controlId} .js-results`);

            $results.removeClass("alert-danger").removeClass("alert-warning").removeClass("alert-success");

            if (status.Errors && this.errors.length > 0) {
                $results.addClass("alert-danger");
            }
            else if (status.Warnings && this.warnings.length > 0) {
                $results.addClass("alert-warning");
            }
            else {
                $results.addClass("alert-success");
            }

            $results.text(status.Message).slideDown();
            $preparing.slideUp();
            $progress.slideUp();
        };

        TaskActivityProgressReporter.prototype.onUpdateTaskProgress = function (progress) {
            const taskId = this.getTaskId();

            if (progress.TaskId !== taskId) {
                return;
            }

            const $preparing = $(`#${this.controlId} .js-preparing`);
            const $progress = $(`#${this.controlId} .js-progress-div`);
            const $bar = $(`#${this.controlId} .js-progress-bar`);

            $bar.prop("aria-valuenow", progress.CompletionPercentage);
            $bar.prop("aria-valuemax", "100");
            $bar.css("width", `${progress.CompletionPercentage}%`);

            if (progress.Message) {
                $bar.text(progress.Message);
            }
            else {
                $bar.text(`${progress.CompletionPercentage}%`);
            }

            $progress.slideDown();
            $preparing.slideUp();
        };

        const reporters = {};

        const exports = {
            initialize(options) {
                if (!options.controlId) {
                    throw new Error("id is required.");
                }

                if (reporters[options.controlId]) {
                    return;
                }

                reporters[options.controlId] = new TaskActivityProgressReporter(options);
            }
        };

        return exports;
    }());
}(jQuery));
