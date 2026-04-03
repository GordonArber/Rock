# Transaction Failure Monitor — Lava Application

A Lava Application dashboard for detecting financial transactions that were charged by the payment gateway but failed to save to the Rock database. Provides a date-filterable view with dismiss/notification capabilities.

---

## Setup Instructions

### 1. Create a SystemCommunication

Go to **Admin Tools > Communications > System Communications** and create a new communication:

| Field | Value |
|-------|-------|
| **Title** | Transaction Failure Notification |
| **From** | *(use your org default or specify)* |
| **Subject** | `Failed to Save Transaction(s) - {{ 'Now' \| Date:'MMMM d, yyyy' }}` |
| **Body** | *(see [SystemCommunication Email Body](#systemcommunication-email-body) below)* |

After saving, **copy the SystemCommunication's Guid** — you'll need it for the ConfigurationRigging.

### 2. Create the Lava Application

Go to **Admin Tools > CMS Configuration > Lava Applications** and create a new application:

| Field | Value |
|-------|-------|
| **Name** | Transaction Failure Monitor |
| **Slug** | `transaction-failure-monitor` |
| **Is Active** | Yes |
| **Configuration Rigging** | *(see [ConfigurationRigging JSON](#configurationrigging-json) below)* |

### 3. Create Endpoints

On the application detail page, create the following three endpoints:

#### Endpoint 1: check-failures

| Field | Value |
|-------|-------|
| **Name** | Check Failures |
| **Slug** | `check-failures` |
| **HTTP Method** | GET |
| **Enabled Lava Commands** | Sql |
| **Code Template** | *(see [check-failures Endpoint](#get-check-failures-endpoint) below)* |

#### Endpoint 2: send-notification

| Field | Value |
|-------|-------|
| **Name** | Send Notification |
| **Slug** | `send-notification` |
| **HTTP Method** | POST |
| **Enabled Lava Commands** | Sql,Execute |
| **Code Template** | *(see [send-notification Endpoint](#post-send-notification-endpoint) below)* |

#### Endpoint 3: dismiss-failures

| Field | Value |
|-------|-------|
| **Name** | Dismiss Failures |
| **Slug** | `dismiss-failures` |
| **HTTP Method** | POST |
| **Enabled Lava Commands** | Sql |
| **Code Template** | *(see [dismiss-failures Endpoint](#post-dismiss-failures-endpoint) below)* |

### 4. Create the Dismiss Tracking Table

Run this SQL in **Admin Tools > Power Tools > SQL Command**:

```sql
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TransactionFailureDismissal')
BEGIN
    CREATE TABLE [dbo].[TransactionFailureDismissal] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ExceptionLogId] INT NOT NULL,
        [DismissedByPersonAliasId] INT NULL,
        [DismissedDateTime] DATETIME NOT NULL DEFAULT GETDATE(),
        [WasNotificationSent] BIT NOT NULL DEFAULT 0
    );
    CREATE INDEX [IX_ExceptionLogId] ON [dbo].[TransactionFailureDismissal]([ExceptionLogId]);
END
```

### 5. Create a Page

1. Go to **Admin Tools > CMS Configuration > Pages**
2. Create a new page under an appropriate section (e.g., Finance):
   - **Name:** Transaction Failure Monitor
   - **Route:** `finance/transaction-failure-monitor`
3. Add a **Lava Application Content** block to the page
4. Configure the block:
   - **Application:** Transaction Failure Monitor
   - **Lava Template:** *(see [Content Block Template](#content-block-template) below)*

---

## ConfigurationRigging JSON

Update these values for your environment before pasting into the Lava Application's Configuration Rigging field:

```json
{
    "GivingSupportEmail": "giving@life.church",
    "LeaderEmail": "",
    "RockBaseUrl": "https://your-rock-instance.com",
    "SystemCommunicationGuid": "REPLACE-WITH-YOUR-SYSTEM-COMMUNICATION-GUID"
}
```

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
LEFT JOIN [TransactionFailureDismissal] tfd ON tfd.[ExceptionLogId] = el.[Id]
WHERE el.[Description] LIKE 'Error recording charge:%'
    AND el.[ExceptionType] = 'System.Exception'
    AND el.[CreatedDateTime] >= '{{ filterDate }}'
    AND el.[CreatedDateTime] < '{{ nextDate }}'
    AND tfd.[Id] IS NULL
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
                            <a href="{{ ConfigurationRigging.RockBaseUrl }}/person/{{ person.Id }}" target="_blank">
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
LEFT JOIN [TransactionFailureDismissal] tfd ON tfd.[ExceptionLogId] = el.[Id]
WHERE el.[Description] LIKE 'Error recording charge:%'
    AND el.[ExceptionType] = 'System.Exception'
    AND el.[CreatedDateTime] >= '{{ filterDate }}'
    AND el.[CreatedDateTime] < '{{ nextDate }}'
    AND tfd.[Id] IS NULL
ORDER BY el.[Id] DESC
{% endsql %}

{% assign failureCount = results | Size %}

{% if failureCount == 0 %}
<div class="alert alert-info">
    <i class="fa fa-info-circle"></i> No failures to send notification for.
</div>
{% else %}

{% comment %} Build a pipe-delimited data string for each failure to pass into execute block {% endcomment %}
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

{% execute import:'Rock.Communication,Rock.Data,Rock.Model,System.Collections.Generic' %}
    var systemCommGuidStr = "{{ ConfigurationRigging.SystemCommunicationGuid }}";
    var givingSupportEmail = "{{ ConfigurationRigging.GivingSupportEmail }}";
    var leaderEmail = "{{ ConfigurationRigging.LeaderEmail }}";
    var rockBaseUrl = "{{ ConfigurationRigging.RockBaseUrl }}";
    var failureDataStr = "{{ failureData }}";

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

    var systemCommGuid = systemCommGuidStr.AsGuidOrNull();
    if ( systemCommGuid == null )
    {
        return "ERROR: Invalid SystemCommunication Guid in ConfigurationRigging.";
    }

    var mergeFields = new Dictionary<string, object>();
    mergeFields.Add( "FailedTransactions", failures );
    mergeFields.Add( "RockBaseUrl", rockBaseUrl );
    mergeFields.Add( "FailureCount", failures.Count );

    var recipients = new List<RockEmailMessageRecipient>();

    if ( !string.IsNullOrWhiteSpace( givingSupportEmail ) )
    {
        recipients.Add( RockEmailMessageRecipient.CreateAnonymous( givingSupportEmail.Trim(), mergeFields ) );
    }

    if ( !string.IsNullOrWhiteSpace( leaderEmail ) )
    {
        recipients.Add( RockEmailMessageRecipient.CreateAnonymous( leaderEmail.Trim(), mergeFields ) );
    }

    if ( recipients.Count == 0 )
    {
        return "ERROR: No recipient emails configured in ConfigurationRigging.";
    }

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

{% comment %} Auto-dismiss failures that were sent {% endcomment %}
{% sql statement:'command' %}
    INSERT INTO [TransactionFailureDismissal] ([ExceptionLogId], [DismissedDateTime], [WasNotificationSent])
    SELECT el.[Id], GETDATE(), 1
    FROM [ExceptionLog] el
    WHERE el.[Id] IN ({{ exceptionIdList }})
    AND NOT EXISTS (
        SELECT 1 FROM [TransactionFailureDismissal] tfd WHERE tfd.[ExceptionLogId] = el.[Id]
    )
{% endsql %}

<div class="alert alert-success">
    <i class="fa fa-check-circle"></i> Notification email sent successfully for <strong>{{ failureCount }}</strong> transaction(s).
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
        IF NOT EXISTS (SELECT 1 FROM [TransactionFailureDismissal] WHERE [ExceptionLogId] = {{ id }})
        BEGIN
            INSERT INTO [TransactionFailureDismissal] ([ExceptionLogId], [DismissedDateTime], [WasNotificationSent])
            VALUES ({{ id }}, GETDATE(), 0)
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
LEFT JOIN [TransactionFailureDismissal] tfd ON tfd.[ExceptionLogId] = el.[Id]
WHERE el.[Description] LIKE 'Error recording charge:%'
    AND el.[ExceptionType] = 'System.Exception'
    AND el.[CreatedDateTime] >= '{{ filterDate }}'
    AND el.[CreatedDateTime] < '{{ nextDate }}'
    AND tfd.[Id] IS NULL
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
                            <a href="{{ ConfigurationRigging.RockBaseUrl }}/person/{{ person.Id }}" target="_blank">
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
                    <a href="{{ RockBaseUrl }}/person/{{ txn.PersonId }}">{{ txn.PersonName }}</a>
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

- **Security:** Ensure only appropriate staff have Execute access to this Lava Application. The SQL queries and execute blocks have powerful permissions.
- **Dismiss tracking:** Uses a custom `TransactionFailureDismissal` table rather than modifying the ExceptionLog table directly. This keeps the ExceptionLog clean and avoids any issues with Rock's built-in exception handling.
- **Recipient configuration:** Update `GivingSupportEmail` and `LeaderEmail` in the Lava Application's Configuration Rigging whenever recipients change — no code modification needed.
- **PagerDuty integration (future):** The `send-notification` endpoint could be extended to use `{% webrequest %}` to call the PagerDuty API (`/oncalls` endpoint) to dynamically fetch the on-call person's email before sending.
