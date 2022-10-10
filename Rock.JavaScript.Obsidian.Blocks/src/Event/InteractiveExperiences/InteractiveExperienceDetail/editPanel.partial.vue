<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <fieldset>
        <div class="row">
            <div class="col-md-6">
                <TextBox v-model="name"
                         label="Name"
                         rules="required" />
            </div>

            <div class="col-md-6">
                <CheckBox v-model="isActive"
                          label="Active" />
            </div>
        </div>

        <TextBox v-model="description"
                 label="Description"
                 textMode="multiline" />

        <div class="row">
            <div class="col-md-6">
                <TextBox v-model="publicLabel"
                         label="Public Label"
                         rules="required"
                         help="The experience name that will be shown publicly." />
            </div>

            <div class="col-md-6">
                <ImageUploader v-model="experiencePhoto"
                               label="Experience Photo"
                               help="An optional photo to be used when displaying information about the experience."
                               :binaryFileTypeGuid="binaryFileTypeGuid" />
            </div>
        </div>

        <div class="mb-5">
            <RadioButtonList v-model="pushNotificationConfiguration"
                             label="Push Notification Configuration"
                             help="Detemines when push notifications should be sent when launching actions."
                             :items="pushNotificationConfigurationItems"
                             horizontal />

            <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
        </div>

        <SectionContainer title="Schedules"
                          description="Schedules determine when and where the experience occurs. You can also configure filters to determine who should be allowed to view an experience. These filters require that the individual is logged in.">
            <table class="grid-table table table-condensed table-light">
                <thead>
                    <tr align="left">
                        <th>Schedule</th>
                        <th>Campus</th>
                        <th>Data View</th>
                        <th>Group</th>
                        <th class="grid-columncommand"></th>
                        <th class="grid-columncommand"></th>
                    </tr>
                </thead>

                <tbody>
                    <tr v-for="row in schedules" align="left">
                        <td>{{ row.schedule?.text }}</td>
                        <td>{{ getScheduleCampusNames(row) }}</td>
                        <td>{{ row.dataView?.text }}</td>
                        <td>{{ row.group?.text }}</td>
                        <td class="grid-columncommand" align="center">
                            <a class="btn btn-sm grid-edit-button">
                                <i class="fa fa-pencil"></i>
                            </a>
                        </td>
                        <td class="grid-columncommand" align="center">
                            <a class="btn btn-danger btn-sm grid-delete-button">
                                <i class="fa fa-times"></i>
                            </a>
                        </td>
                    </tr>

                    <tr v-if="!schedules.length" align="left">
                        <td colspan="6">No schedules defined.</td>
                    </tr>
                </tbody>

                <tfoot>
                    <tr>
                        <td class="grid-actions" colspan="6">
                            <a class="btn btn-grid-action btn-add btn-default btn-sm"
                               accesskey="n"
                               title="Alt+N"
                               href="#"
                               @click.prevent="onAddScheduleClick">
                                <i class="fa fa-plus-circle fa-fw"></i>
                            </a>
                        </td>
                    </tr>
                </tfoot>
            </table>
        </SectionContainer>

        <SectionContainer title="Welcome Content"
                          description="This optional content will be shown before the first action is displayed. It allows you to welcome your guest to the environment and let them know what to expect.">
            <div class="row">
                <div class="col-md-6">
                    <TextBox v-model="welcomeTitle"
                             label="Title" />

                    <TextBox v-model="welcomeMessage"
                             label="Message"
                             textMode="multiline" />
                </div>

                <div class="col-md-6">
                    <ImageUploader v-model="welcomeHeaderImage"
                                   label="Header Image"
                                   :binaryFileTypeGuid="binaryFileTypeGuid" />
                </div>
            </div>
        </SectionContainer>

        <SectionContainer title="No Actions Content"
                          description="This optional content will be shown when there are no active actions being displayed.">
            <div class="row">
                <div class="col-md-6">
                    <TextBox v-model="noActionsTitle"
                             label="Title" />

                    <TextBox v-model="noActionsMessage"
                             label="Message"
                             textMode="multiline" />
                </div>

                <div class="col-md-6">
                    <ImageUploader v-model="noActionsHeaderImage"
                                   label="Header Image"
                                   :binaryFileTypeGuid="binaryFileTypeGuid" />
                </div>
            </div>
        </SectionContainer>

        <SectionContainer title="Action Appearance"
                          description="The settings below can help override the default appearance on the individual devices to provide a custom theme for the experience.">
            <div class="row">
                <div class="col-lg-3 col-sm-6">
                    <ColorPicker v-model="actionBackgroundColor"
                                 label="Background Color" />
                </div>

                <div class="col-lg-3 col-sm-6">
                    <ColorPicker v-model="actionTextColor"
                                 label="Text Color" />
                </div>

                <div class="col-lg-3 col-sm-6">
                    <ColorPicker v-model="actionPrimaryButtonColor"
                                 label="Primary Button Color" />
                </div>

                <div class="col-lg-3 col-sm-6">
                    <ColorPicker v-model="actionPrimaryButtonTextColor"
                                 label="Primary Button Text Color" />
                </div>
            </div>

            <div class="row">
                <div class="col-sm-6"></div>

                <div class="col-lg-3 col-sm-6">
                    <ColorPicker v-model="actionSecondaryButtonColor"
                                 label="Secondary Button Color" />
                </div>

                <div class="col-lg-3 col-sm-6">
                    <ColorPicker v-model="actionSecondaryButtonTextColor"
                                 label="Secondary Button Text Color" />
                </div>
            </div>

            <ImageUploader v-model="actionBackgroundImage"
                           label="Background Image"
                           :binaryFileTypeGuid="binaryFileTypeGuid" />

            <div class="mt-2 text-right">
                <a href="#" class="text-xs" @click.prevent="onActionAdvancedOptionsClick">
                    Advanced Options
                    <i v-if="isActionAdvancedOptionsVisible" class="fa fa-angle-up"></i>
                    <i v-else class="fa fa-angle-down"></i>
                </a>
            </div>

            <TransitionVerticalCollapse>
                <div v-if="isActionAdvancedOptionsVisible">
                    <CodeEditor v-model="actionCustomCss"
                                label="Custom CSS"
                                mode="css" />
                </div>
            </TransitionVerticalCollapse>
        </SectionContainer>

        <SectionContainer title="Audience Appearance"
                          description="The settings below can help override the default appearance on the audience visuals to provide a custom theme for the experience.">
            <div class="row">
                <div class="col-lg-3 col-sm-6">
                    <ColorPicker v-model="audienceBackgroundColor"
                                 label="Background Color" />
                </div>

                <div class="col-lg-3 col-sm-6">
                    <ColorPicker v-model="audienceTextColor"
                                 label="Text Color" />
                </div>
            </div>

            <div class="row">
                <div class="col-lg-3 col-sm-6">
                    <ColorPicker v-model="audiencePrimaryColor"
                                 label="Primary Color" />
                </div>

                <div class="col-lg-3 col-sm-6">
                    <ColorPicker v-model="audienceSecondaryColor"
                                 label="Secondary Color" />
                </div>

                <div class="col-lg-3 col-sm-6">
                    <ColorPicker v-model="audienceAccentColor"
                                 label="Accent Color" />
                </div>
            </div>

            <ImageUploader v-model="audienceBackgroundImage"
                           label="Background Image"
                           :binaryFileTypeGuid="binaryFileTypeGuid" />

            <div class="mt-2 text-right">
                <a href="#" class="text-xs" @click.prevent="onAudienceAdvancedOptionsClick">
                    Advanced Options
                    <i v-if="isAudienceAdvancedOptionsVisible" class="fa fa-angle-up"></i>
                    <i v-else class="fa fa-angle-down"></i>
                </a>
            </div>

            <TransitionVerticalCollapse>
                <div v-if="isAudienceAdvancedOptionsVisible">
                    <CodeEditor v-model="audienceCustomCss"
                                label="Custom CSS"
                                mode="css" />
                </div>
            </TransitionVerticalCollapse>
        </SectionContainer>
    </fieldset>
</template>

<script setup lang="ts">
    import { PropType, ref, watch } from "vue";
    import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
    import CheckBox from "@Obsidian/Controls/checkBox";
    import CodeEditor from "@Obsidian/Controls/codeEditor";
    import ColorPicker from "@Obsidian/Controls/colorPicker";
    import ImageUploader from "@Obsidian/Controls/imageUploader";
    import RadioButtonList from "@Obsidian/Controls/radioButtonList";
    import SectionContainer from "@Obsidian/Controls/sectionContainer";
    import TextBox from "@Obsidian/Controls/textBox";
    import TransitionVerticalCollapse from "@Obsidian/Controls/transitionVerticalCollapse";
    import { watchPropertyChanges } from "@Obsidian/Utility/block";
    import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
    import { BinaryFiletype } from "@Obsidian/SystemGuids";
    import { InteractiveExperienceBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceBag";
    import { InteractiveExperienceDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceDetailOptionsBag";
    import { toNumber } from "@Obsidian/Utility/numberUtils";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { InteractiveExperiencePushNotificationType } from "@Obsidian/Enums/Event/interactiveExperiencePushNotificationType";
    import { InteractiveExperienceScheduleBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceScheduleBag";

    const props = defineProps({
        modelValue: {
            type: Object as PropType<InteractiveExperienceBag>,
            required: true
        },

        options: {
            type: Object as PropType<InteractiveExperienceDetailOptionsBag>,
            required: true
        }
    });

    const emit = defineEmits<{
        (e: "update:modelValue", value: InteractiveExperienceBag): void,
        (e: "propertyChanged", value: string): void
    }>();

    // #region Values

    const attributes = ref(props.modelValue.attributes ?? {});
    const attributeValues = ref(props.modelValue.attributeValues ?? {});
    const description = propertyRef(props.modelValue.description ?? "", "Description");
    const isActive = propertyRef(props.modelValue.isActive ?? false, "IsActive");
    const name = propertyRef(props.modelValue.name ?? "", "Name");
    const publicLabel = propertyRef(props.modelValue.publicLabel ?? "", "PublicLabel");
    const experiencePhoto = propertyRef(props.modelValue.photoBinaryFile ?? null, "PhotoBinaryFileId");
    const pushNotificationConfiguration = propertyRef(props.modelValue.pushNotificationType.toString(), "PushNotificationType");
    const schedules = ref(props.modelValue.schedules ?? []);
    const welcomeTitle = propertyRef(props.modelValue.welcomeTitle ?? "", "WelcomeTitle");
    const welcomeMessage = propertyRef(props.modelValue.welcomeMessage ?? "", "WelcomeMessage");
    const welcomeHeaderImage = propertyRef(props.modelValue.welcomeHeaderImageBinaryFile ?? null, "WelcomeHeaderImageBinaryFileId");
    const noActionsTitle = propertyRef(props.modelValue.noActionTitle ?? "", "NoActionTitle");
    const noActionsMessage = propertyRef(props.modelValue.noActionMessage ?? "", "NoActionMessage");
    const noActionsHeaderImage = propertyRef(props.modelValue.noActionHeaderImageBinaryFile ?? null, "NoActionHeaderImageBinaryFileId");
    const actionBackgroundColor = propertyRef(props.modelValue.actionBackgroundColor ?? "", "ActionBackgroundColor");
    const actionTextColor = propertyRef(props.modelValue.actionTextColor ?? "", "ActionTextColor");
    const actionPrimaryButtonColor = propertyRef(props.modelValue.actionPrimaryButtonColor ?? "", "ActionPrimaryButtonColor");
    const actionPrimaryButtonTextColor = propertyRef(props.modelValue.actionPrimaryButtonTextColor ?? "", "ActionPrimaryButtonTextColor");
    const actionSecondaryButtonColor = propertyRef(props.modelValue.actionSecondaryButtonColor ?? "", "ActionSecondaryButtonColor");
    const actionSecondaryButtonTextColor = propertyRef(props.modelValue.actionSecondaryButtonTextColor ?? "", "ActionSecondaryButtonTextColor");
    const actionBackgroundImage = propertyRef(props.modelValue.actionBackgroundImageBinaryFile ?? null, "ActionBackgroundImageBinaryFileId");
    const actionCustomCss = propertyRef(props.modelValue.actionCustomCss ?? "", "ActionCustomCss");
    const audienceBackgroundColor = propertyRef(props.modelValue.audienceBackgroundColor ?? "", "ActionBackgroundColor");
    const audienceTextColor = propertyRef(props.modelValue.audienceTextColor ?? "", "ActionTextColor");
    const audiencePrimaryColor = propertyRef(props.modelValue.audiencePrimaryColor ?? "", "AudiencePrimaryColor");
    const audienceSecondaryColor = propertyRef(props.modelValue.audienceSecondaryColor ?? "", "AudienceSecondaryColor");
    const audienceAccentColor = propertyRef(props.modelValue.audienceAccentColor ?? "", "AudienceAccentColor");
    const audienceBackgroundImage = propertyRef(props.modelValue.audienceBackgroundImageBinaryFile ?? null, "AudienceBackgroundImageBinaryFileId");
    const audienceCustomCss = propertyRef(props.modelValue.audienceCustomCss ?? "", "AudienceCustomCss");

    // The properties that are being edited. This should only contain
    // objects returned by propertyRef().
    const propRefs = [description,
        isActive,
        name,
        publicLabel,
        experiencePhoto,
        pushNotificationConfiguration,
        welcomeTitle,
        welcomeMessage,
        welcomeHeaderImage,
        noActionsTitle,
        noActionsMessage,
        noActionsHeaderImage,
        actionBackgroundColor,
        actionTextColor,
        actionPrimaryButtonColor,
        actionPrimaryButtonTextColor,
        actionSecondaryButtonColor,
        actionSecondaryButtonTextColor,
        actionBackgroundImage,
        actionCustomCss,
        audienceBackgroundColor,
        audienceTextColor,
        audiencePrimaryColor,
        audienceSecondaryColor,
        audienceAccentColor,
        audienceBackgroundImage,
        audienceCustomCss];

    const binaryFileTypeGuid = BinaryFiletype.Default;
    const isActionAdvancedOptionsVisible = ref(false);
    const isAudienceAdvancedOptionsVisible = ref(false);

    const pushNotificationConfigurationItems: ListItemBag[] = [
        {
            value: InteractiveExperiencePushNotificationType.Never.toString(),
            text: "Never"
        },
        {
            value: InteractiveExperiencePushNotificationType.EveryAction.toString(),
            text: "Every Action"
        },
        {
            value: InteractiveExperiencePushNotificationType.SpecificActions.toString(),
            text: "Specific Actions"
        }
    ];

    // #endregion

    // #region Computed Values

    // #endregion

    // #region Functions

    function getScheduleCampusNames(schedule: InteractiveExperienceScheduleBag): string {
        if (schedule.campuses && schedule.campuses.length) {
            return schedule.campuses.map(c => c.text ?? "").join(", ");
        }
        else {
            return "";
        }
    }

    // #endregion

    // #region Event Handlers

    function onAddScheduleClick(): void {

    }

    function onActionAdvancedOptionsClick(): void {
        isActionAdvancedOptionsVisible.value = !isActionAdvancedOptionsVisible.value;
    }

    function onAudienceAdvancedOptionsClick(): void {
        isAudienceAdvancedOptionsVisible.value = !isAudienceAdvancedOptionsVisible.value;
    }

    // #endregion

    // Watch for parental changes in our model value and update all our values.
    watch(() => props.modelValue, () => {
        updateRefValue(attributes, props.modelValue.attributes ?? {});
        updateRefValue(attributeValues, props.modelValue.attributeValues ?? {});
        updateRefValue(description, props.modelValue.description ?? "");
        updateRefValue(isActive, props.modelValue.isActive ?? false);
        updateRefValue(name, props.modelValue.name ?? "");
        updateRefValue(publicLabel, props.modelValue.publicLabel ?? "");
        updateRefValue(experiencePhoto, props.modelValue.photoBinaryFile ?? null);
        updateRefValue(pushNotificationConfiguration, props.modelValue.pushNotificationType.toString());
        updateRefValue(welcomeTitle, props.modelValue.welcomeTitle ?? "");
        updateRefValue(welcomeMessage, props.modelValue.welcomeMessage ?? "");
        updateRefValue(welcomeHeaderImage, props.modelValue.welcomeHeaderImageBinaryFile ?? null);
        updateRefValue(noActionsTitle, props.modelValue.noActionTitle ?? "");
        updateRefValue(noActionsMessage, props.modelValue.noActionMessage ?? "");
        updateRefValue(noActionsHeaderImage, props.modelValue.noActionHeaderImageBinaryFile ?? null);
        updateRefValue(actionBackgroundColor, props.modelValue.actionBackgroundColor ?? "");
        updateRefValue(actionTextColor, props.modelValue.actionTextColor ?? "");
        updateRefValue(actionPrimaryButtonColor, props.modelValue.actionPrimaryButtonColor ?? "");
        updateRefValue(actionPrimaryButtonTextColor, props.modelValue.actionPrimaryButtonTextColor ?? "");
        updateRefValue(actionSecondaryButtonColor, props.modelValue.actionSecondaryButtonColor ?? "");
        updateRefValue(actionSecondaryButtonTextColor, props.modelValue.actionSecondaryButtonTextColor ?? "");
        updateRefValue(actionBackgroundImage, props.modelValue.actionBackgroundImageBinaryFile ?? null);
        updateRefValue(actionCustomCss, props.modelValue.actionCustomCss ?? "");
        updateRefValue(audienceBackgroundColor, props.modelValue.audienceBackgroundColor ?? "");
        updateRefValue(audienceTextColor, props.modelValue.audienceTextColor ?? "");
        updateRefValue(audiencePrimaryColor, props.modelValue.audiencePrimaryColor ?? "");
        updateRefValue(audienceSecondaryColor, props.modelValue.audienceSecondaryColor ?? "");
        updateRefValue(audienceAccentColor, props.modelValue.audienceAccentColor ?? "");
        updateRefValue(audienceBackgroundImage, props.modelValue.audienceBackgroundImageBinaryFile ?? null);
        updateRefValue(audienceCustomCss, props.modelValue.audienceCustomCss ?? "");
    });

    // Determines which values we want to track changes on (defined in the
    // array) and then emit a new object defined as newValue.
    watch([attributeValues, schedules, ...propRefs], () => {
        const newValue: InteractiveExperienceBag = {
            ...props.modelValue,
            attributeValues: attributeValues.value,
            description: description.value,
            isActive: isActive.value,
            name: name.value,
            publicLabel: publicLabel.value,
            photoBinaryFile: experiencePhoto.value,
            pushNotificationType: toNumber(pushNotificationConfiguration.value),
            welcomeTitle: welcomeTitle.value,
            welcomeMessage: welcomeMessage.value,
            welcomeHeaderImageBinaryFile: welcomeHeaderImage.value,
            noActionTitle: noActionsTitle.value,
            noActionMessage: noActionsMessage.value,
            noActionHeaderImageBinaryFile: noActionsHeaderImage.value,
            actionBackgroundColor: actionBackgroundColor.value,
            actionTextColor: actionTextColor.value,
            actionPrimaryButtonColor: actionPrimaryButtonColor.value,
            actionPrimaryButtonTextColor: actionPrimaryButtonTextColor.value,
            actionSecondaryButtonColor: actionSecondaryButtonColor.value,
            actionSecondaryButtonTextColor: actionSecondaryButtonTextColor.value,
            actionBackgroundImageBinaryFile: actionBackgroundImage.value,
            actionCustomCss: actionCustomCss.value,
            audienceBackgroundColor: audienceBackgroundColor.value,
            audienceTextColor: audienceTextColor.value,
            audiencePrimaryColor: audiencePrimaryColor.value,
            audienceSecondaryColor: audienceSecondaryColor.value,
            audienceAccentColor: audienceAccentColor.value,
            audienceBackgroundImageBinaryFile: audienceBackgroundImage.value,
            audienceCustomCss: audienceCustomCss.value
        };

        emit("update:modelValue", newValue);
    });

    // Watch for any changes to props that represent properties and then
    // automatically emit which property changed.
    watchPropertyChanges(propRefs, emit);

    // Watch for changes to name, and if the old value matches the public name
    // then update the public name to the new value.
    watch(name, (newValue, oldValue) => {
        if (oldValue === publicLabel.value) {
            publicLabel.value = newValue;
        }
    });
</script>
