# Transaction Failure Monitor — Lava Application

A Lava Application dashboard for detecting financial transactions that were charged by the payment gateway but failed to save to the Rock database. Provides a date-filterable view with dismiss/notification capabilities. Sends email notifications to the giving support team, a configurable leader, and the current PagerDuty on-call engineer.

---

## Setup Instructions

### 1. Create a Defined Type for Recipients

Go to **Admin Tools > General Settings > Defined Types** and create a new Defined Type:

| Field | Value |
|-------|-------|
| **Name** | Transaction Failure Notification Recipients |
| **Description** | Email addresses that receive notifications when financial transactions fail to save to the database. Each Defined Value should be an email address. Manage recipients by adding or removing values here — no code changes needed. |
| **Category** | Finance |

Add the following **Defined Values**:

| Value | Description |
|-------|-------------|
| `giving@life.church` | Giving support team inbox |
| *(leader's email)* | Finance team leader — update this value when leadership changes |

After saving, **copy the Defined Type's Guid** — you'll need it for the ConfigurationRigging.

### 2. Create a SystemCommunication

Go to **Admin Tools > Communications > System Communications** and create a new communication:

| Field | Value |
|-------|-------|
| **Title** | Transaction Failure Notification |
| **Description** | Sent when financial transactions are charged by the payment gateway but fail to save to the Rock database. Contains a table of affected transactions with person profile links, timestamps, amounts, and transaction GUIDs. |
| **Category** | Finance |
| **From** | *(use your org default or specify)* |
| **Subject** | `Failed to Save Transaction(s) - {{ 'Now' \| Date:'MMMM d, yyyy' }}` |
| **Body** | *(see [SystemCommunication Email Body](#systemcommunication-email-body) below)* |

After saving, **copy the SystemCommunication's Guid** — you'll need it for the ConfigurationRigging.

### 3. Create an Entity Attribute for Dismiss Tracking

Go to **Admin Tools > System Settings > Entity Attributes** and create a new attribute:

| Field | Value |
|-------|-------|
| **Entity Type** | Exception Log |
| **Name** | Is Transaction Failure Dismissed |
| **Key** | `IsTransactionFailureDismissed` |
| **Description** | Indicates this exception has been reviewed and dismissed (or emailed) by the Transaction Failure Monitor. Set to True when a failure is dismissed or included in a notification email. |
| **Field Type** | Boolean |
| **Default Value** | False |

### 4. Create the Lava Application

Go to **Admin Tools > CMS Configuration > Lava Applications** and create a new application:

| Field | Value |
|-------|-------|
| **Name** | Transaction Failure Monitor |
| **Description** | Monitors ExceptionLog for financial transactions that were charged by the payment gateway but failed to save to the Rock database. Provides a dashboard to review failures and send notification emails to the giving support team, leader, and on-call engineer. |
| **Slug** | `transaction-failure-monitor` |
| **Is Active** | Yes |
| **Configuration Rigging** | *(see [ConfigurationRigging JSON](#configurationrigging-json) below)* |

### 5. Create Endpoints

On the application detail page, create the following three endpoints:

#### Endpoint 1: check-failures

| Field | Value |
|-------|-------|
| **Name** | Check Failures |
| **Description** | Queries ExceptionLog for "Error recording charge:" entries on the selected date, parses transaction details (Person, Amount, Guid), and renders an HTML table. Excludes previously dismissed entries. |
| **Slug** | `check-failures` |
| **HTTP Method** | GET |
| **Enabled Lava Commands** | Sql |
| **Security Mode** | Application View |
| **Enable Cross-Site Forgery Protection** | Unchecked *(read-only GET request)* |
| **Cacheability Type** | No-Store |
| **Code Template** | *(see [check-failures Endpoint](#get-check-failures-endpoint) below)* |

#### Endpoint 2: send-notification

| Field | Value |
|-------|-------|
| **Name** | Send Notification |
| **Description** | Sends a notification email to all configured recipients (from Defined Type) plus the current PagerDuty on-call engineer. Includes all non-dismissed failures for the selected date. Auto-dismisses failures after successful send. |
| **Slug** | `send-notification` |
| **HTTP Method** | POST |
| **Enabled Lava Commands** | Sql,Execute,WebRequest |
| **Security Mode** | Application Edit |
| **Enable Cross-Site Forgery Protection** | Checked |
| **Cacheability Type** | No-Store |
| **Code Template** | *(see [send-notification Endpoint](#post-send-notification-endpoint) below)* |

#### Endpoint 3: dismiss-failures

| Field | Value |
|-------|-------|
| **Name** | Dismiss Failures |
| **Description** | Marks selected ExceptionLog entries as dismissed so they no longer appear in the failure list. Used when a failure has been manually resolved or is a known issue that does not need a notification email. |
| **Slug** | `dismiss-failures` |
| **HTTP Method** | POST |
| **Enabled Lava Commands** | Sql |
| **Security Mode** | Application Edit |
| **Enable Cross-Site Forgery Protection** | Checked |
| **Cacheability Type** | No-Store |
| **Code Template** | *(see [dismiss-failures Endpoint](#post-dismiss-failures-endpoint) below)* |

### 6. Create a Page

1. Go to **Admin Tools > CMS Configuration > Pages**
2. Create a new page under an appropriate section (e.g., Finance):
   - **Name:** Transaction Failure Monitor
   - **Description:** Dashboard for reviewing financial transactions that failed to save to the database after being charged by the payment gateway. Allows sending notification emails and dismissing resolved failures.
   - **Route:** `finance/transaction-failure-monitor`
3. Add a **Lava Application Content** block to the page
4. Configure the block:
   - **Name:** Transaction Failure Monitor Content
   - **Application:** Transaction Failure Monitor
   - **Lava Template:** *(see [Content Block Template](#content-block-template) below)*

---

## ConfigurationRigging JSON

Update these values for your environment before pasting into the Lava Application's Configuration Rigging field:

```json
{
    "SystemCommunicationGuid": "REPLACE-WITH-YOUR-SYSTEM-COMMUNICATION-GUID",
    "RecipientDefinedTypeGuid": "REPLACE-WITH-YOUR-DEFINED-TYPE-GUID",
    "PagerDutyApiToken": "",
    "PagerDutyScheduleId": ""
}
```

| Key | Description |
|-----|-------------|
| `SystemCommunicationGuid` | The Guid of the SystemCommunication template created in step 2. |
| `RecipientDefinedTypeGuid` | The Guid of the "Transaction Failure Notification Recipients" Defined Type created in step 1. |
| `PagerDutyApiToken` | API token from PagerDuty (Account Settings > API Access Keys). Leave empty to skip PagerDuty integration. |
| `PagerDutyScheduleId` | The PagerDuty schedule ID to look up the current on-call engineer. Leave empty to skip. |

---

## Content Block Template

Paste this into the **Lava Template** field of the LavaApplicationContent block:

```html
<div id="txn-failure-monitor">
    <div class="panel panel-block">
        <div class="panel-heading">
            <h1 class="panel-title"><i class="fa fa-exclamation-triangle"></i> Transaction Failure Monitor</h1>
        </div>
        <div class="panel-body">
            <p class="text-muted mb-3">
                Review financial transactions that were charged by the payment gateway but failed to save to the database.
                Use the date filter to check specific days. Dismiss resolved items or send a notification email to the team.
            </p>
            <div class="row mb-3">
                <div class="col-md-4">
                    <label for="filterDate">Date</label>
                    <input type="date"
                           id="filterDate"
                           name="filterDate"
                           class="form-control"
                           hx-get="^/transaction-failure-monitor/check-failures"
                           hx-target="#failure-results"
                           hx-trigger="change"
                           hx-include="#filterDate" />
                </div>
            </div>

            <div id="failure-results"
                 hx-get="^/transaction-failure-monitor/check-failures"
                 hx-trigger="load"
                 hx-swap="innerHTML">
                <div class="text-center p-4">
                    <i class="fa fa-spinner fa-spin"></i> Loading...
                </div>
            </div>
        </div>
    </div>
</div>

<script>
    // Default the date input to today
    document.addEventListener('DOMContentLoaded', function() {
        var dateInput = document.getElementById('filterDate');
        if (dateInput && !dateInput.value) {
            var today = new Date();
            var yyyy = today.getFullYear();
            var mm = String(today.getMonth() + 1).padStart(2, '0');
            var dd = String(today.getDate()).padStart(2, '0');
            dateInput.value = yyyy + '-' + mm + '-' + dd;
        }
    });
</script>
```

---

## GET check-failures Endpoint

Paste this into the **Code Template** field of the `check-failures` endpoint:

```
{% assign filterDate = QueryString.filterDate %}
{% if filterDate == null or filterDate == '' %}
    {% assign filterDate = 'Now' | Date:'yyyy-MM-dd' %}
{% endif %}

{% assign nextDate = filterDate | Date:'yyyy-MM-dd' | DateAdd:1,'d' | Date:'yyyy-MM-dd' %}

{% sql results %}
SELECT
    el.[Id],
    el.[CreatedDateTime],
    el.[Description]
FROM [ExceptionLog] el
LEFT JOIN [Attribute] a
    ON a.[Key] = 'IsTransactionFailureDismissed'
    AND a.[EntityTypeId] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.ExceptionLog')
LEFT JOIN [AttributeValue] av
    ON av.[AttributeId] = a.[Id]
    AND av.[EntityId] = el.[Id]
    AND av.[Value] = 'True'
WHERE el.[Description] LIKE 'Error recording charge:%'
    AND el.[ExceptionType] = 'System.Exception'
    AND el.[CreatedDateTime] >= '{{ filterDate }}'
    AND el.[CreatedDateTime] < '{{ nextDate }}'
    AND av.[Id] IS NULL
ORDER BY el.[Id] DESC
{% endsql %}

{% assign failureCount = results | Size %}

{% if failureCount == 0 %}
<div class="alert alert-success">
    <i class="fa fa-check-circle"></i> No unresolved failed transactions found for {{ filterDate | Date:'MMMM d, yyyy' }}.
</div>
{% else %}
<div class="alert alert-warning">
    <i class="fa fa-exclamation-triangle"></i> <strong>{{ failureCount }}</strong> failed transaction(s) found for {{ filterDate | Date:'MMMM d, yyyy' }}.
</div>

<form id="failure-form">
    <input type="hidden" name="filterDate" value="{{ filterDate }}" />
    <table class="table table-bordered table-striped table-hover">
        <thead>
            <tr>
                <th style="width: 40px;">
                    <input type="checkbox" id="selectAll" onclick="
                        var checkboxes = document.querySelectorAll('input[name=exceptionIds]');
                        for (var i = 0; i < checkboxes.length; i++) {
                            checkboxes[i].checked = this.checked;
                        }
                    " />
                </th>
                <th>Person</th>
                <th>Time</th>
                <th>Amount</th>
                <th>Transaction Guid</th>
            </tr>
        </thead>
        <tbody>
            {% for row in results %}
                {% assign desc = row.Description %}

                {% comment %} Parse Person ID from "Person {id}" {% endcomment %}
                {% assign personSplit = desc | Split:', Person ' %}
                {% assign personId = '' %}
                {% if personSplit.size > 1 %}
                    {% assign personAndRest = personSplit[1] | Split:', Exception' %}
                    {% assign personId = personAndRest[0] | Trim %}
                {% endif %}

                {% comment %} Parse Amount from "Total Amount {amount}" {% endcomment %}
                {% assign amountSplit = desc | Split:', Total Amount ' %}
                {% assign amount = '' %}
                {% if amountSplit.size > 1 %}
                    {% assign amountAndRest = amountSplit[1] | Split:', Person' %}
                    {% assign amount = amountAndRest[0] | Trim %}
                {% endif %}

                {% comment %} Parse Guid from "Guid {guid}" {% endcomment %}
                {% assign guidSplit = desc | Split:'Guid ' %}
                {% assign txnGuid = '' %}
                {% if guidSplit.size > 1 %}
                    {% assign guidAndRest = guidSplit[1] | Split:', Total Amount' %}
                    {% assign txnGuid = guidAndRest[0] | Trim %}
                {% endif %}

                {% comment %} Look up person name {% endcomment %}
                {% if personId != '' %}
                    {% sql personResult %}
                        SELECT TOP 1 p.[NickName], p.[LastName], p.[Id]
                        FROM [Person] p
                        WHERE p.[Id] = {{ personId }}
                    {% endsql %}
                    {% assign person = personResult | First %}
                {% endif %}

                <tr>
                    <td>
                        <input type="checkbox" name="exceptionIds" value="{{ row.Id }}" />
                    </td>
                    <td>
                        {% if person %}
                            <a href="{{ 'Global' | Attribute:'InternalApplicationRoot' }}person/{{ person.Id }}" target="_blank">
                                {{ person.NickName }} {{ person.LastName }}
                            </a>
                        {% elseif personId != '' %}
                            Person {{ personId }}
                        {% else %}
                            Unknown
                        {% endif %}
                    </td>
                    <td>{{ row.CreatedDateTime | Date:'M/d/yyyy h:mm tt' }}</td>
                    <td>{{ amount }}</td>
                    <td><code>{{ txnGuid }}</code></td>
                </tr>
            {% endfor %}
        </tbody>
    </table>

    <div class="actions mt-3">
        <button type="button"
                class="btn btn-primary"
                hx-post="^/transaction-failure-monitor/send-notification"
                hx-target="#failure-results"
                hx-include="#failure-form"
                hx-confirm="Send notification email for all displayed failures?">
            <i class="fa fa-envelope"></i> Send Notification Email
        </button>
        <button type="button"
                class="btn btn-default ml-2"
                hx-post="^/transaction-failure-monitor/dismiss-failures"
                hx-target="#failure-results"
                hx-include="#failure-form"
                hx-confirm="Dismiss selected failures? They will no longer appear in the list.">
            <i class="fa fa-times-circle"></i> Dismiss Selected
        </button>
    </div>
</form>
{% endif %}
```

---

## POST send-notification Endpoint

Paste this into the **Code Template** field of the `send-notification` endpoint:

```
{% assign filterDate = Form.filterDate %}
{% if filterDate == null or filterDate == '' %}
    {% assign filterDate = 'Now' | Date:'yyyy-MM-dd' %}
{% endif %}
{% assign nextDate = filterDate | Date:'yyyy-MM-dd' | DateAdd:1,'d' | Date:'yyyy-MM-dd' %}

{% comment %} Fetch all non-dismissed failures for the selected date {% endcomment %}
{% sql results %}
SELECT
    el.[Id],
    el.[CreatedDateTime],
    el.[Description]
FROM [ExceptionLog] el
LEFT JOIN [Attribute] a
    ON a.[Key] = 'IsTransactionFailureDismissed'
    AND a.[EntityTypeId] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.ExceptionLog')
LEFT JOIN [AttributeValue] av
    ON av.[AttributeId] = a.[Id]
    AND av.[EntityId] = el.[Id]
    AND av.[Value] = 'True'
WHERE el.[Description] LIKE 'Error recording charge:%'
    AND el.[ExceptionType] = 'System.Exception'
    AND el.[CreatedDateTime] >= '{{ filterDate }}'
    AND el.[CreatedDateTime] < '{{ nextDate }}'
    AND av.[Id] IS NULL
ORDER BY el.[Id] DESC
{% endsql %}

{% assign failureCount = results | Size %}

{% if failureCount == 0 %}
<div class="alert alert-info">
    <i class="fa fa-info-circle"></i> No failures to send notification for.
</div>
{% else %}

{% comment %} ============================================= {% endcomment %}
{% comment %} Build failure data for the execute block       {% endcomment %}
{% comment %} ============================================= {% endcomment %}
{% assign failureData = '' %}
{% assign exceptionIdList = '' %}
{% for row in results %}
    {% assign desc = row.Description %}

    {% assign personSplit = desc | Split:', Person ' %}
    {% assign personId = '' %}
    {% if personSplit.size > 1 %}
        {% assign personAndRest = personSplit[1] | Split:', Exception' %}
        {% assign personId = personAndRest[0] | Trim %}
    {% endif %}

    {% assign amountSplit = desc | Split:', Total Amount ' %}
    {% assign amount = '' %}
    {% if amountSplit.size > 1 %}
        {% assign amountAndRest = amountSplit[1] | Split:', Person' %}
        {% assign amount = amountAndRest[0] | Trim %}
    {% endif %}

    {% assign guidSplit = desc | Split:'Guid ' %}
    {% assign txnGuid = '' %}
    {% if guidSplit.size > 1 %}
        {% assign guidAndRest = guidSplit[1] | Split:', Total Amount' %}
        {% assign txnGuid = guidAndRest[0] | Trim %}
    {% endif %}

    {% if personId != '' %}
        {% sql personResult %}
            SELECT TOP 1 p.[NickName], p.[LastName], p.[Id]
            FROM [Person] p
            WHERE p.[Id] = {{ personId }}
        {% endsql %}
        {% assign person = personResult | First %}
    {% endif %}

    {% assign personName = 'Unknown' %}
    {% if person %}
        {% assign personName = person.NickName | Append:' ' | Append:person.LastName %}
    {% endif %}

    {% comment %} Format: PersonId|PersonName|Time|Amount|Guid {% endcomment %}
    {% capture entry %}{{ personId }}|{{ personName }}|{{ row.CreatedDateTime | Date:'M/d/yyyy h:mm tt' }}|{{ amount }}|{{ txnGuid }}{% endcapture %}

    {% if failureData != '' %}
        {% assign failureData = failureData | Append:'~~' | Append:entry %}
    {% else %}
        {% assign failureData = entry %}
    {% endif %}

    {% if exceptionIdList != '' %}
        {% assign exceptionIdList = exceptionIdList | Append:',' | Append:row.Id %}
    {% else %}
        {% capture exceptionIdList %}{{ row.Id }}{% endcapture %}
    {% endif %}
{% endfor %}

{% comment %} ============================================= {% endcomment %}
{% comment %} Fetch PagerDuty on-call engineer email         {% endcomment %}
{% comment %} ============================================= {% endcomment %}
{% assign pagerDutyEmail = '' %}
{% assign pdToken = ConfigurationRigging.PagerDutyApiToken %}
{% assign pdScheduleId = ConfigurationRigging.PagerDutyScheduleId %}

{% if pdToken != '' and pdScheduleId != '' %}
    {% webrequest url:'https://api.pagerduty.com/oncalls?schedule_ids[]={{ pdScheduleId }}' headers:'Authorization: Token token={{ pdToken }},Accept: application/vnd.pagerduty+json;version=2,Content-Type: application/json' return:'pdResponse' responsecontenttype:'json' %}
    {% endwebrequest %}

    {% if pdResponse.oncalls and pdResponse.oncalls.size > 0 %}
        {% assign pagerDutyEmail = pdResponse.oncalls[0].user.email %}
    {% endif %}
{% endif %}

{% comment %} ============================================= {% endcomment %}
{% comment %} Fetch static recipient emails from Defined Type {% endcomment %}
{% comment %} ============================================= {% endcomment %}
{% assign recipientEmails = '' %}
{% sql recipientResults %}
SELECT dv.[Value]
FROM [DefinedValue] dv
INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
WHERE dt.[Guid] = '{{ ConfigurationRigging.RecipientDefinedTypeGuid }}'
    AND dv.[IsActive] = 1
ORDER BY dv.[Order]
{% endsql %}

{% for recipient in recipientResults %}
    {% if recipientEmails != '' %}
        {% assign recipientEmails = recipientEmails | Append:',' | Append:recipient.Value %}
    {% else %}
        {% assign recipientEmails = recipient.Value %}
    {% endif %}
{% endfor %}

{% comment %} Add PagerDuty on-call email if found {% endcomment %}
{% if pagerDutyEmail != '' %}
    {% if recipientEmails != '' %}
        {% assign recipientEmails = recipientEmails | Append:',' | Append:pagerDutyEmail %}
    {% else %}
        {% assign recipientEmails = pagerDutyEmail %}
    {% endif %}
{% endif %}

{% comment %} ============================================= {% endcomment %}
{% comment %} Send the notification email                    {% endcomment %}
{% comment %} ============================================= {% endcomment %}
{% execute import:'Rock.Communication,Rock.Data,Rock.Model,System.Collections.Generic' %}
    var systemCommGuidStr = "{{ ConfigurationRigging.SystemCommunicationGuid }}";
    var recipientEmailsStr = "{{ recipientEmails }}";
    var failureDataStr = "{{ failureData }}";
    var internalAppRoot = "{{ 'Global' | Attribute:'InternalApplicationRoot' }}";

    // Parse failure data
    var failures = new List<Dictionary<string, string>>();
    var entries = failureDataStr.Split( new string[] { "~~" }, StringSplitOptions.RemoveEmptyEntries );

    foreach ( var entry in entries )
    {
        var parts = entry.Split( '|' );
        if ( parts.Length >= 5 )
        {
            failures.Add( new Dictionary<string, string>
            {
                { "PersonId", parts[0] },
                { "PersonName", parts[1] },
                { "Time", parts[2] },
                { "Amount", parts[3] },
                { "TransactionGuid", parts[4] }
            } );
        }
    }

    // Validate SystemCommunication Guid
    var systemCommGuid = systemCommGuidStr.AsGuidOrNull();
    if ( systemCommGuid == null )
    {
        return "ERROR: Invalid SystemCommunication Guid in ConfigurationRigging.";
    }

    // Build merge fields
    var mergeFields = new Dictionary<string, object>();
    mergeFields.Add( "FailedTransactions", failures );
    mergeFields.Add( "FailureCount", failures.Count );
    mergeFields.Add( "InternalApplicationRoot", internalAppRoot );

    // Build recipient list
    var recipients = new List<RockEmailMessageRecipient>();
    var emails = recipientEmailsStr.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

    foreach ( var email in emails )
    {
        var trimmedEmail = email.Trim();
        if ( !string.IsNullOrWhiteSpace( trimmedEmail ) )
        {
            recipients.Add( RockEmailMessageRecipient.CreateAnonymous( trimmedEmail, mergeFields ) );
        }
    }

    if ( recipients.Count == 0 )
    {
        return "ERROR: No recipient emails found. Check the Defined Type and PagerDuty configuration.";
    }

    // Send email
    var emailMessage = new RockEmailMessage( systemCommGuid.Value );
    emailMessage.SetRecipients( recipients );
    emailMessage.CreateCommunicationRecord = false;

    var errorMessages = new List<string>();
    var success = emailMessage.Send( out errorMessages );

    if ( !success )
    {
        return "ERROR: " + string.Join( ", ", errorMessages );
    }

    return "OK";
{% endexecute %}

{% comment %} ============================================= {% endcomment %}
{% comment %} Auto-dismiss failures that were sent           {% endcomment %}
{% comment %} ============================================= {% endcomment %}
{% sql statement:'command' %}
    DECLARE @AttributeId INT = (
        SELECT TOP 1 a.[Id]
        FROM [Attribute] a
        INNER JOIN [EntityType] et ON et.[Id] = a.[EntityTypeId]
        WHERE a.[Key] = 'IsTransactionFailureDismissed'
            AND et.[Name] = 'Rock.Model.ExceptionLog'
    )

    IF @AttributeId IS NOT NULL
    BEGIN
        INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
        SELECT 0, @AttributeId, el.[Id], 'True', NEWID()
        FROM [ExceptionLog] el
        WHERE el.[Id] IN ({{ exceptionIdList }})
            AND NOT EXISTS (
                SELECT 1
                FROM [AttributeValue] av
                WHERE av.[AttributeId] = @AttributeId
                    AND av.[EntityId] = el.[Id]
            )
    END
{% endsql %}

<div class="alert alert-success">
    <i class="fa fa-check-circle"></i> Notification email sent successfully for <strong>{{ failureCount }}</strong> transaction(s).
    {% if pagerDutyEmail != '' %}
        <br /><small>PagerDuty on-call: {{ pagerDutyEmail }}</small>
    {% endif %}
</div>

{% endif %}
```

---

## POST dismiss-failures Endpoint

Paste this into the **Code Template** field of the `dismiss-failures` endpoint:

```
{% assign selectedIds = Form.exceptionIds %}
{% assign filterDate = Form.filterDate %}

{% if selectedIds == null or selectedIds == '' %}
<div class="alert alert-warning">
    <i class="fa fa-info-circle"></i> No failures were selected. Check the boxes next to the rows you want to dismiss.
</div>
{% else %}

{% comment %} Handle both single and multiple checkbox values {% endcomment %}
{% assign idArray = selectedIds | Split:',' %}
{% for id in idArray %}
    {% sql statement:'command' %}
        DECLARE @AttributeId INT = (
            SELECT TOP 1 a.[Id]
            FROM [Attribute] a
            INNER JOIN [EntityType] et ON et.[Id] = a.[EntityTypeId]
            WHERE a.[Key] = 'IsTransactionFailureDismissed'
                AND et.[Name] = 'Rock.Model.ExceptionLog'
        )

        IF @AttributeId IS NOT NULL
        AND NOT EXISTS (
            SELECT 1 FROM [AttributeValue]
            WHERE [AttributeId] = @AttributeId AND [EntityId] = {{ id }}
        )
        BEGIN
            INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
            VALUES (0, @AttributeId, {{ id }}, 'True', NEWID())
        END
    {% endsql %}
{% endfor %}

<div class="alert alert-success mb-3">
    <i class="fa fa-check-circle"></i> {{ idArray | Size }} failure(s) dismissed.
</div>

{% endif %}

{% comment %} Re-render the remaining failures {% endcomment %}
{% if filterDate == null or filterDate == '' %}
    {% assign filterDate = 'Now' | Date:'yyyy-MM-dd' %}
{% endif %}
{% assign nextDate = filterDate | Date:'yyyy-MM-dd' | DateAdd:1,'d' | Date:'yyyy-MM-dd' %}

{% sql results %}
SELECT
    el.[Id],
    el.[CreatedDateTime],
    el.[Description]
FROM [ExceptionLog] el
LEFT JOIN [Attribute] a
    ON a.[Key] = 'IsTransactionFailureDismissed'
    AND a.[EntityTypeId] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.ExceptionLog')
LEFT JOIN [AttributeValue] av
    ON av.[AttributeId] = a.[Id]
    AND av.[EntityId] = el.[Id]
    AND av.[Value] = 'True'
WHERE el.[Description] LIKE 'Error recording charge:%'
    AND el.[ExceptionType] = 'System.Exception'
    AND el.[CreatedDateTime] >= '{{ filterDate }}'
    AND el.[CreatedDateTime] < '{{ nextDate }}'
    AND av.[Id] IS NULL
ORDER BY el.[Id] DESC
{% endsql %}

{% assign failureCount = results | Size %}

{% if failureCount == 0 %}
<div class="alert alert-success">
    <i class="fa fa-check-circle"></i> No unresolved failed transactions found for {{ filterDate | Date:'MMMM d, yyyy' }}.
</div>
{% else %}
<div class="alert alert-warning">
    <i class="fa fa-exclamation-triangle"></i> <strong>{{ failureCount }}</strong> failed transaction(s) remaining for {{ filterDate | Date:'MMMM d, yyyy' }}.
</div>

<form id="failure-form">
    <input type="hidden" name="filterDate" value="{{ filterDate }}" />
    <table class="table table-bordered table-striped table-hover">
        <thead>
            <tr>
                <th style="width: 40px;">
                    <input type="checkbox" id="selectAll" onclick="
                        var checkboxes = document.querySelectorAll('input[name=exceptionIds]');
                        for (var i = 0; i < checkboxes.length; i++) {
                            checkboxes[i].checked = this.checked;
                        }
                    " />
                </th>
                <th>Person</th>
                <th>Time</th>
                <th>Amount</th>
                <th>Transaction Guid</th>
            </tr>
        </thead>
        <tbody>
            {% for row in results %}
                {% assign desc = row.Description %}

                {% assign personSplit = desc | Split:', Person ' %}
                {% assign personId = '' %}
                {% if personSplit.size > 1 %}
                    {% assign personAndRest = personSplit[1] | Split:', Exception' %}
                    {% assign personId = personAndRest[0] | Trim %}
                {% endif %}

                {% assign amountSplit = desc | Split:', Total Amount ' %}
                {% assign amount = '' %}
                {% if amountSplit.size > 1 %}
                    {% assign amountAndRest = amountSplit[1] | Split:', Person' %}
                    {% assign amount = amountAndRest[0] | Trim %}
                {% endif %}

                {% assign guidSplit = desc | Split:'Guid ' %}
                {% assign txnGuid = '' %}
                {% if guidSplit.size > 1 %}
                    {% assign guidAndRest = guidSplit[1] | Split:', Total Amount' %}
                    {% assign txnGuid = guidAndRest[0] | Trim %}
                {% endif %}

                {% if personId != '' %}
                    {% sql personResult %}
                        SELECT TOP 1 p.[NickName], p.[LastName], p.[Id]
                        FROM [Person] p
                        WHERE p.[Id] = {{ personId }}
                    {% endsql %}
                    {% assign person = personResult | First %}
                {% endif %}

                <tr>
                    <td>
                        <input type="checkbox" name="exceptionIds" value="{{ row.Id }}" />
                    </td>
                    <td>
                        {% if person %}
                            <a href="{{ 'Global' | Attribute:'InternalApplicationRoot' }}person/{{ person.Id }}" target="_blank">
                                {{ person.NickName }} {{ person.LastName }}
                            </a>
                        {% elseif personId != '' %}
                            Person {{ personId }}
                        {% else %}
                            Unknown
                        {% endif %}
                    </td>
                    <td>{{ row.CreatedDateTime | Date:'M/d/yyyy h:mm tt' }}</td>
                    <td>{{ amount }}</td>
                    <td><code>{{ txnGuid }}</code></td>
                </tr>
            {% endfor %}
        </tbody>
    </table>

    <div class="actions mt-3">
        <button type="button"
                class="btn btn-primary"
                hx-post="^/transaction-failure-monitor/send-notification"
                hx-target="#failure-results"
                hx-include="#failure-form"
                hx-confirm="Send notification email for all displayed failures?">
            <i class="fa fa-envelope"></i> Send Notification Email
        </button>
        <button type="button"
                class="btn btn-default ml-2"
                hx-post="^/transaction-failure-monitor/dismiss-failures"
                hx-target="#failure-results"
                hx-include="#failure-form"
                hx-confirm="Dismiss selected failures? They will no longer appear in the list.">
            <i class="fa fa-times-circle"></i> Dismiss Selected
        </button>
    </div>
</form>
{% endif %}
```

---

## SystemCommunication Email Body

Paste this into the **Body** field of the SystemCommunication template:

```html
{{ 'Global' | Attribute:'EmailHeader' }}

<h2 style="color: #ee7625;">Failed Transaction Notification</h2>

<p>Hey Team,</p>

<p>We failed to save <strong>{{ FailureCount }}</strong> transaction(s). The affected transactions are below.</p>

<table style="width: 100%; border-collapse: collapse; margin: 20px 0;" cellpadding="8" cellspacing="0">
    <thead>
        <tr style="background-color: #f5f5f5;">
            <th style="border: 1px solid #ddd; text-align: left; padding: 10px;">Person</th>
            <th style="border: 1px solid #ddd; text-align: left; padding: 10px;">Time</th>
            <th style="border: 1px solid #ddd; text-align: left; padding: 10px;">Amount</th>
            <th style="border: 1px solid #ddd; text-align: left; padding: 10px;">Transaction Guid</th>
        </tr>
    </thead>
    <tbody>
        {% for txn in FailedTransactions %}
        <tr>
            <td style="border: 1px solid #ddd; padding: 10px;">
                {% if txn.PersonId != '' %}
                    <a href="{{ InternalApplicationRoot }}person/{{ txn.PersonId }}">{{ txn.PersonName }}</a>
                {% else %}
                    {{ txn.PersonName }}
                {% endif %}
            </td>
            <td style="border: 1px solid #ddd; padding: 10px;">{{ txn.Time }}</td>
            <td style="border: 1px solid #ddd; padding: 10px;">{{ txn.Amount }}</td>
            <td style="border: 1px solid #ddd; padding: 10px;"><code>{{ txn.TransactionGuid }}</code></td>
        </tr>
        {% endfor %}
    </tbody>
</table>

<p><em>This notification was sent from the Transaction Failure Monitor Lava Application.</em></p>

{{ 'Global' | Attribute:'EmailFooter' }}
```

---

## Notes

- **Security:** Ensure only appropriate staff have Execute access to this Lava Application. The SQL queries and execute blocks have powerful permissions. POST endpoints require Application Edit security.
- **Dismiss tracking:** Uses Rock's built-in Entity Attribute system (`AttributeValue` table) rather than a custom table. The `IsTransactionFailureDismissed` boolean attribute on ExceptionLog is set to `True` when an entry is dismissed or included in a notification email.
- **Recipient management:** Static recipients (giving@life.church, leader) are managed via the "Transaction Failure Notification Recipients" Defined Type. Admins add/remove Defined Values — no code or configuration changes needed.
- **PagerDuty integration:** The `send-notification` endpoint calls the PagerDuty `/oncalls` API to dynamically fetch the on-call engineer's email. If the PagerDuty token or schedule ID is empty in ConfigurationRigging, this step is silently skipped.
- **Person profile links:** All links use `{{ 'Global' | Attribute:'InternalApplicationRoot' }}` for the base URL.
- **Error source:** The exception description format is defined in `Rock/Financial/AutomatedPaymentProcessor.cs:682`. If that format ever changes, the Lava parsing logic in the endpoints will need to be updated.
