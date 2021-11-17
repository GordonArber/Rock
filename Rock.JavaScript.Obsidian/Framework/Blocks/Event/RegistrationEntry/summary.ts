﻿// <copyright>
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

import { defineComponent, inject, ref } from "vue";
import GatewayControl, { GatewayControlModel, prepareSubmitPayment } from "../../../Controls/gatewayControl";
import { useInvokeBlockAction } from "../../../Util/block";
import RockForm from "../../../Controls/rockForm";
import RockValidation from "../../../Controls/rockValidation";
import Alert from "../../../Elements/alert";
import CheckBox from "../../../Elements/checkBox";
import EmailBox from "../../../Elements/emailBox";
import RockButton from "../../../Elements/rockButton";
import DropDownList, { DropDownListOption } from "../../../Elements/dropDownList";
import { getRegistrantBasicInfo, RegistrantBasicInfo, RegistrationEntryState } from "../registrationEntry";
import CostSummary from "./costSummary";
import DiscountCodeForm from "./discountCodeForm";
import Registrar from "./registrar";
import { RegistrationEntryBlockArgs } from "./registrationEntryBlockArgs";
import { RegistrationEntryBlockSuccessViewModel, RegistrationEntryBlockViewModel } from "./registrationEntryBlockViewModel";
import { toGuidOrNull } from "../../../Util/guid";

export default defineComponent({
    name: "Event.RegistrationEntry.Summary",
    components: {
        RockButton,
        CheckBox,
        EmailBox,
        RockForm,
        Alert,
        DropDownList,
        GatewayControl,
        RockValidation,
        CostSummary,
        Registrar,
        DiscountCodeForm
    },
    setup() {
        const submitPayment = prepareSubmitPayment();

        const getRegistrationEntryBlockArgs = inject("getRegistrationEntryBlockArgs") as () => RegistrationEntryBlockArgs;
        const invokeBlockAction = useInvokeBlockAction();
        const registrationEntryState = inject("registrationEntryState") as RegistrationEntryState;

        /** Is there an AJAX call in-flight? */
        const loading = ref(false);

        /** Gateway indicated error */
        const gatewayErrorMessage = ref("");

        /** Gateway indicated validation issues */
        const gatewayValidationFields = ref<Record<string, string>>({});

        /** An error message received from a bad submission */
        const submitErrorMessage = ref("");

        /** The currently selected saved account. */
        const selectedSavedAccount = ref("");

        if (registrationEntryState.viewModel.savedAccounts !== null && registrationEntryState.viewModel.savedAccounts.length > 0) {
            selectedSavedAccount.value = registrationEntryState.viewModel.savedAccounts[0].value;
        }

        return {
            loading,
            gatewayErrorMessage,
            gatewayValidationFields,
            submitErrorMessage,
            selectedSavedAccount,
            submitPayment,
            getRegistrationEntryBlockArgs,
            invokeBlockAction,
            registrationEntryState: registrationEntryState
        };
    },

    computed: {
        /** The settings for the gateway (MyWell, etc) control */
        gatewayControlModel(): GatewayControlModel {
            return this.viewModel.gatewayControl;
        },

        /** This is the data sent from the C# code behind when the block initialized. */
        viewModel(): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.viewModel;
        },

        /** Info about the registrants made available by .FirstName instead of by field guid */
        registrantInfos(): RegistrantBasicInfo[] {
            return this.registrationEntryState.registrants.map(r => getRegistrantBasicInfo(r, this.viewModel.registrantForms));
        },

        /** The registrant term - plural if there are more than 1 */
        registrantTerm(): string {
            return this.registrantInfos.length === 1 ? this.viewModel.registrantTerm : this.viewModel.pluralRegistrantTerm;
        },

        /** The name of this registration instance */
        instanceName(): string {
            return this.viewModel.instanceName;
        },

        /** The text to be displayed on the "Finish" button */
        finishButtonText(): string {
            return (this.viewModel.isRedirectGateway && this.registrationEntryState.amountToPayToday) ? "Pay" : "Finish";
        },

        /** true if there are any saved accounts to be selected. */
        hasSavedAccounts(): boolean {
            return this.registrationEntryState.viewModel.savedAccounts !== null
                && this.registrationEntryState.viewModel.savedAccounts.length > 0;
        },

        /** Contains the options to display in the saved account drop down list. */
        savedAccountOptions(): DropDownListOption[] {
            if (this.registrationEntryState.viewModel.savedAccounts === null) {
                return [];
            }

            const options = this.registrationEntryState.viewModel.savedAccounts.map(a => {
                const option: DropDownListOption = {
                    value: a.value,
                    text: a.text
                };

                return option;
            });

            options.push({
                value: "",
                text: "Use a different payment method"
            });

            return options;
        },

        /** true if the gateway control should be visible. */
        showGateway(): boolean {
            return !this.hasSavedAccounts || this.selectedSavedAccount === "";
        }
    },

    methods: {
        /** User clicked the "previous" button */
        onPrevious() {
            this.$emit("previous");
        },

        /** User clicked the "finish" button */
        async onNext() {
            this.loading = true;

            // If there is a cost, then the gateway will need to be used to pay
            if (this.registrationEntryState.amountToPayToday) {
                // If this is a redirect gateway, then persist and redirect now
                if (this.viewModel.isRedirectGateway) {
                    const redirectUrl = await this.getPaymentRedirect();

                    if (redirectUrl) {
                        location.href = redirectUrl;
                    }
                    else {
                        // Error is shown by getPaymentRedirect method
                        this.loading = false;
                    }
                }
                else if (this.showGateway) {
                    // Otherwise, this is a traditional gateway
                    this.gatewayErrorMessage = "";
                    this.gatewayValidationFields = {};
                    this.submitPayment();
                }
                else if (this.selectedSavedAccount !== "") {
                    this.registrationEntryState.savedAccountGuid = toGuidOrNull(this.selectedSavedAccount);
                    const success = await this.submit();
                    this.loading = false;

                    if (success) {
                        this.$emit("next");
                    }
                }
                else {
                    this.submitErrorMessage = "Please select a valid payment option.";
                    this.loading = false;

                    return;
                }
            }
            else {
                const success = await this.submit();
                this.loading = false;

                if (success) {
                    this.$emit("next");
                }
            }
        },

        /**
         * The gateway indicated success and returned a token
         * @param token
         */
        async onGatewayControlSuccess(token: string) {
            this.registrationEntryState.gatewayToken = token;
            const success = await this.submit();

            this.loading = false;

            if (success) {
                this.$emit("next");
            }
        },

        /**
         * The gateway indicated an error
         * @param message
         */
        onGatewayControlError(message: string) {
            this.loading = false;
            this.gatewayErrorMessage = message;
        },

        /**
         * The gateway wants the user to fix some fields
         * @param invalidFields
         */
        onGatewayControlValidation(invalidFields: Record<string, string>) {
            this.loading = false;
            this.gatewayValidationFields = invalidFields;
        },

        /** Submit the registration to the server */
        async submit(): Promise<boolean> {
            this.submitErrorMessage = "";

            const result = await this.invokeBlockAction<RegistrationEntryBlockSuccessViewModel>("SubmitRegistration", {
                args: this.getRegistrationEntryBlockArgs()
            });

            if (result.isError || !result.data) {
                this.submitErrorMessage = result.errorMessage || "Unknown error";
            }
            else {
                this.registrationEntryState.successViewModel = result.data;
            }

            return result.isSuccess;
        },

        /** Persist the args to the server so the user can be redirected for payment. Returns the redirect URL. */
        async getPaymentRedirect(): Promise<string> {
            const result = await this.invokeBlockAction<string>("GetPaymentRedirect", {
                args: this.getRegistrationEntryBlockArgs()
            });

            if (result.isError || !result.data) {
                this.submitErrorMessage = result.errorMessage || "Unknown error";
            }

            return result.data || "";
        }
    },

    template: `
<div class="registrationentry-summary">
    <RockForm @submit="onNext">

        <Registrar />

        <div v-if="viewModel.cost">
            <h4>Payment Summary</h4>
            <DiscountCodeForm />
            <CostSummary />
        </div>

        <div v-if="gatewayControlModel" v-show="registrationEntryState.amountToPayToday" class="well">
            <h4>Payment Method</h4>
            <DropDownList v-if="hasSavedAccounts" v-model="selectedSavedAccount" :options="savedAccountOptions" :showBlankItem="false" />
            <div v-show="showGateway">
                <Alert v-if="gatewayErrorMessage" alertType="danger">{{gatewayErrorMessage}}</Alert>
                <RockValidation :errors="gatewayValidationFields" />
                <div class="hosted-payment-control">
                    <GatewayControl
                        :gatewayControlModel="gatewayControlModel"
                        @success="onGatewayControlSuccess"
                        @error="onGatewayControlError"
                        @validation="onGatewayControlValidation" />
                </div>
            </div>
        </div>

        <div v-if="!viewModel.cost" class="margin-b-md">
            <p>The following {{registrantTerm}} will be registered for {{instanceName}}:</p>
            <ul>
                <li v-for="r in registrantInfos" :key="r.guid">
                    <strong>{{r.firstName}} {{r.lastName}}</strong>
                </li>
            </ul>
        </div>

        <Alert v-if="submitErrorMessage" alertType="danger">{{submitErrorMessage}}</Alert>

        <div class="actions text-right">
            <RockButton v-if="viewModel.allowRegistrationUpdates" class="pull-left" btnType="default" @click="onPrevious" :isLoading="loading">
                Previous
            </RockButton>
            <RockButton btnType="primary" type="submit" :isLoading="loading">
                {{finishButtonText}}
            </RockButton>
        </div>
    </RockForm>
</div>`
});
