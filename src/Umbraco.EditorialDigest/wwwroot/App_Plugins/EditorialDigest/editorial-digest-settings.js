import { umbHttpClient } from "@umbraco-cms/backoffice/http-client";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

const apiRoot = "/umbraco/management/api/v1/editorial-digest";
const defaultConfig = () => ({
    id: 0, name: "", alias: "", description: "", isEnabled: true, recipientSource: 0,
    recipientUserGroups: "", scheduleType: 0, scheduleDay: null, scheduleTime: "09:00:00", timeZoneId: browserTimeZone(),
    sectionsEnabled: [0, 1], lookbackHours: 24, upcomingHours: 48, staleDays: 90, expiringDays: 7,
    maxItemsPerSection: 10, subjectLineTemplate: "{{digestName}} — Editorial Digest for {{date}}",
    fromName: "", fromEmail: "", replyToEmail: "", customTemplatePath: ""
});

class EditorialDigestSettings extends UmbElementMixin(HTMLElement) {
    async connectedCallback() {
        await this.load();
    }

    async load(selectedId) {
        try {
            const [configs, settings] = await Promise.all([this.request("/configurations"), this.request("/settings")]);
            this.configs = configs;
            this.settings = settings;
            this.editor = selectedId ? await this.request(`/configurations/${selectedId}`) : this.editor;
            this.render();
        } catch (error) {
            this.innerHTML = `<uui-box headline="Editorial Digest"><p role="alert">${escapeHtml(error.message)}</p></uui-box>`;
        }
    }

    render() {
        this.innerHTML = `<style>${styles}</style>${this.editor ? editView(this.editor) : listView(this.configs, this.settings)}`;
        this.bindEvents();
    }

    bindEvents() {
        this.querySelector("#new-config")?.addEventListener("click", () => { this.editor = defaultConfig(); this.render(); });
        this.querySelectorAll("[data-back-to-list]").forEach(button => button.addEventListener("click", () => { this.editor = null; this.render(); }));
        this.querySelectorAll("[data-edit]").forEach(button => button.addEventListener("click", () => this.load(Number(button.dataset.edit))));
        this.querySelector("#global-settings")?.addEventListener("submit", event => this.saveGlobal(event));
        this.querySelector("#digest-config")?.addEventListener("submit", event => this.saveConfig(event));
        this.querySelector("#run-now")?.addEventListener("click", () => this.runNow());
        this.querySelector("#preview")?.addEventListener("click", () => window.open(`${apiRoot}/configurations/${this.editor.id}/delivery/preview`, "_blank", "noopener"));
        this.querySelector("#duplicate")?.addEventListener("click", () => this.duplicate(this.editor.id));
        this.querySelector("#delete")?.addEventListener("click", () => this.remove(this.editor.id));
        this.bindFormAssistance();
    }

    bindFormAssistance() {
        const form = this.querySelector("#digest-config");
        if (!form) return;

        const name = form.elements.namedItem("name");
        const alias = form.elements.namedItem("alias");
        name.addEventListener("input", () => {
            if (!this.editor.id && alias.dataset.edited !== "true") alias.value = slugify(name.value);
        });
        alias.addEventListener("input", () => { alias.dataset.edited = "true"; });

        const scheduleType = form.elements.namedItem("scheduleType");
        const scheduleDay = this.querySelector("[data-schedule-day]");
        const updateSchedule = () => { scheduleDay.hidden = scheduleType.value !== "1"; };
        scheduleType.addEventListener("change", updateSchedule);
        updateSchedule();

        const recipientSource = form.elements.namedItem("recipientSource");
        const groups = this.querySelector("[data-recipient-groups]");
        const updateRecipients = () => { groups.hidden = recipientSource.value === "1"; };
        recipientSource.addEventListener("change", updateRecipients);
        updateRecipients();
    }

    async request(path, options = {}) {
        return request(path, options);
    }

    async saveGlobal(event) {
        event.preventDefault();
        const form = new FormData(event.currentTarget);
        await this.execute(() => this.request("/settings", { method: "PUT", body: JSON.stringify({
            defaultFromName: form.get("defaultFromName"), defaultFromEmail: form.get("defaultFromEmail") || null,
            logoUrl: form.get("logoUrl") || null, customTemplateBasePath: form.get("customTemplateBasePath") || null,
            dashboardRefreshMinutes: Number(form.get("dashboardRefreshMinutes")), isPackageEnabled: form.get("isPackageEnabled") === "on",
            loggingLevel: Number(form.get("loggingLevel"))
        }) }).then(() => this.load()));
    }

    async saveConfig(event) {
        event.preventDefault();
        const form = new FormData(event.currentTarget);
        const sectionsEnabled = [...event.currentTarget.querySelectorAll("[name=sectionsEnabled]:checked")].map(input => Number(input.value));
        const payload = {
            ...defaultConfig(),
            name: form.get("name"), alias: form.get("alias"), description: form.get("description") || null,
            isEnabled: form.get("isEnabled") === "on", recipientSource: Number(form.get("recipientSource")),
            recipientUserGroups: form.get("recipientUserGroups") || null, scheduleType: Number(form.get("scheduleType")),
            scheduleDay: form.get("scheduleType") === "1" ? Number(form.get("scheduleDay")) : null, scheduleTime: form.get("scheduleTime"),
            timeZoneId: form.get("timeZoneId"), sectionsEnabled, lookbackHours: Number(form.get("lookbackHours")),
            upcomingHours: Number(form.get("upcomingHours")), staleDays: Number(form.get("staleDays")), expiringDays: Number(form.get("expiringDays")),
            maxItemsPerSection: Number(form.get("maxItemsPerSection")), subjectLineTemplate: form.get("subjectLineTemplate"),
            fromName: form.get("fromName") || null, fromEmail: form.get("fromEmail") || null, replyToEmail: form.get("replyToEmail") || null,
            customTemplatePath: form.get("customTemplatePath") || null
        };
        const id = this.editor.id;
        await this.execute(async () => {
            this.editor = await this.request(`/configurations${id ? `/${id}` : ""}`, { method: id ? "PUT" : "POST", body: JSON.stringify(payload) });
            await this.load(this.editor.id);
        });
    }

    async duplicate(id) { await this.execute(() => this.request(`/configurations/${id}/duplicate`, { method: "POST" }).then(config => this.load(config.id))); }
    async remove(id) { if (confirm("Delete this digest configuration?")) await this.execute(() => this.request(`/configurations/${id}`, { method: "DELETE" }).then(() => { this.editor = null; return this.load(); })); }
    async runNow() { if (confirm("Send this digest to all active recipients now?")) await this.execute(() => this.request(`/configurations/${this.editor.id}/delivery/run`, { method: "POST" }).then(() => this.load(this.editor.id))); }
    async execute(action) { try { await action(); } catch (error) { alert(error.message); } }
}

function listView(configs, settings) { return `<main>
    <header><div><h1>Digests</h1><p>Keep your team in the loop.</p></div><uui-button id="new-config" look="primary" label="New digest">New digest</uui-button></header>
    ${configs.length ? `<div class="digest-list">${configs.map(config => `<button class="digest-row" type="button" data-edit="${config.id}" aria-label="Edit ${escapeHtml(config.name)}"><span><strong>${escapeHtml(config.name)}</strong><small>${escapeHtml(scheduleSummary(config))}</small></span><span class="status${config.isEnabled ? "" : " status--paused"}">${config.isEnabled ? "Active" : "Paused"}</span></button>`).join("")}</div>` : `<section class="empty"><h2>No digests yet</h2><p>Create one daily summary to get started.</p><uui-button id="new-config" look="primary" label="Create digest">Create digest</uui-button></section>`}
    <details class="package-settings"><summary>Package settings <span>${settings.isPackageEnabled ? "Enabled" : "Paused"}</span></summary>${globalSettingsForm(settings)}</details>
</main>`; }

function editView(config) { return `<main>
    <header><div><button data-back-to-list type="button" class="back">← Digests</button><h1>${config.id ? escapeHtml(config.name) : "New digest"}</h1></div></header>
    <form id="digest-config">
        <label>Name<input name="name" required maxlength="255" autocomplete="off" value="${escapeHtml(config.name)}" placeholder="Marketing morning brief"></label>
        <label>Send to<select name="recipientSource"><option value="0" ${selected(config.recipientSource, 0)}>Umbraco user groups</option><option value="1" ${selected(config.recipientSource, 1)}>Custom mailing list</option><option value="2" ${selected(config.recipientSource, 2)}>User groups and mailing list</option></select></label>
        <label data-recipient-groups>Groups<input name="recipientUserGroups" value="${escapeHtml(config.recipientUserGroups)}" placeholder="editors, marketing"></label>
        <div class="schedule"><label>Send<select name="scheduleType"><option value="0" ${selected(config.scheduleType, 0)}>Every day</option><option value="1" ${selected(config.scheduleType, 1)}>Every week</option></select></label><label data-schedule-day>On<select name="scheduleDay">${weekdayOptions(config.scheduleDay ?? 1)}</select></label><label>At<input name="scheduleTime" type="time" step="60" required value="${escapeHtml(config.scheduleTime)}"></label></div>
        <div class="actions"><uui-button data-back-to-list type="button" look="secondary" label="Cancel">Cancel</uui-button><uui-button type="submit" look="primary" label="Save digest">Save digest</uui-button></div>
        <details><summary>More settings</summary>${advancedSettings(config)}</details>
    </form>
</main>`; }

function advancedSettings(config) { return `<div class="advanced">
    <label>Description<textarea name="description" maxlength="1000">${escapeHtml(config.description)}</textarea></label>
    <label>Time zone<input name="timeZoneId" required value="${escapeHtml(config.timeZoneId)}"></label>
    <fieldset><legend>Include</legend><div class="section-list">${sectionCheckboxes(config.sectionsEnabled)}</div></fieldset>
    <div class="grid"><label>Published lookback (hours)<input name="lookbackHours" type="number" min="1" max="720" value="${config.lookbackHours}"></label><label>Upcoming window (hours)<input name="upcomingHours" type="number" min="1" max="720" value="${config.upcomingHours}"></label><label>Stale after (days)<input name="staleDays" type="number" min="1" max="3650" value="${config.staleDays}"></label><label>Expiring window (days)<input name="expiringDays" type="number" min="1" max="365" value="${config.expiringDays}"></label><label>Maximum items<input name="maxItemsPerSection" type="number" min="1" max="50" value="${config.maxItemsPerSection}"></label></div>
    <label>Subject line<input name="subjectLineTemplate" required value="${escapeHtml(config.subjectLineTemplate)}"></label>
    <div class="grid"><label>From name<input name="fromName" value="${escapeHtml(config.fromName)}"></label><label>From email<input name="fromEmail" type="email" value="${escapeHtml(config.fromEmail)}"></label><label>Reply-to email<input name="replyToEmail" type="email" value="${escapeHtml(config.replyToEmail)}"></label></div>
    <label>Alias<input name="alias" required pattern="[a-z0-9-]+" value="${escapeHtml(config.alias)}"></label><label>Custom template path<input name="customTemplatePath" value="${escapeHtml(config.customTemplatePath)}"></label>
    <label class="check"><input name="isEnabled" type="checkbox" ${config.isEnabled ? "checked" : ""}> Active</label>
    ${config.id ? `<div class="actions"><uui-button id="preview" type="button" look="secondary" label="Preview">Preview</uui-button><uui-button id="run-now" type="button" look="secondary" label="Run now">Run now</uui-button><uui-button id="duplicate" type="button" look="secondary" label="Duplicate">Duplicate</uui-button><uui-button id="delete" type="button" look="danger" label="Delete">Delete</uui-button></div>` : ""}
</div>`; }

function globalSettingsForm(settings) { return `<form id="global-settings" class="advanced">
    <label class="check"><input name="isPackageEnabled" type="checkbox" ${settings.isPackageEnabled ? "checked" : ""}> Enable all digests</label>
    <div class="grid"><label>Default sender name<input name="defaultFromName" value="${escapeHtml(settings.defaultFromName)}"></label><label>Default sender email<input name="defaultFromEmail" type="email" value="${escapeHtml(settings.defaultFromEmail)}"></label><label>Logo URL<input name="logoUrl" type="url" value="${escapeHtml(settings.logoUrl)}"></label><label>Dashboard refresh (minutes)<input name="dashboardRefreshMinutes" type="number" min="1" max="60" value="${settings.dashboardRefreshMinutes}"></label><label>Logging level<select name="loggingLevel"><option value="0" ${selected(settings.loggingLevel, 0)}>Minimal</option><option value="1" ${selected(settings.loggingLevel, 1)}>Normal</option><option value="2" ${selected(settings.loggingLevel, 2)}>Verbose</option></select></label></div>
    <label>Template base path<input name="customTemplateBasePath" value="${escapeHtml(settings.customTemplateBasePath)}"></label>
    <div class="actions"><uui-button type="submit" look="primary" label="Save package settings">Save package settings</uui-button></div>
</form>`; }

function sectionCheckboxes(enabled) { return [[0, "Recently published"], [1, "Upcoming scheduled"], [2, "Stuck workflows"], [3, "Pending review"], [4, "Expiring content"], [5, "Stale content"], [6, "Broken links"]].map(([value, label]) => `<label class="check"><input name="sectionsEnabled" type="checkbox" value="${value}" ${enabled.includes(value) ? "checked" : ""}> ${label}</label>`).join(""); }

async function request(path, options = {}) {
    const result = await umbHttpClient[(options.method || "GET").toLowerCase()]({ url: `${apiRoot}${path}`, body: options.body ? JSON.parse(options.body) : undefined, headers: options.headers, security: [{ type: "http", scheme: "bearer" }] });
    if (result.error) throw new Error(result.error.detail || result.error.title || "The request could not be completed.");
    return result.data ?? null;
}

function scheduleSummary(config) { return `${config.scheduleType === 1 ? `Weekly · ${weekdayName(config.scheduleDay)}` : "Daily"} · ${formatTime(config.scheduleTime)}`; }
function weekdayOptions(selectedDay) { return ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"].map((name, value) => `<option value="${value}" ${selected(selectedDay, value)}>${name}</option>`).join(""); }
function weekdayName(value) { return ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"][Number(value)] || "Monday"; }
function browserTimeZone() { return Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC"; }
function slugify(value) { return value.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-+|-+$/g, ""); }
function selected(value, option) { return Number(value) === option ? "selected" : ""; }
function formatTime(value) { return String(value || "").slice(0, 5) || "09:00"; }
function escapeHtml(value) { return String(value ?? "").replace(/[&<>'"]/g, character => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "'": "&#39;", "\"": "&quot;" })[character]); }

const styles = `
    :host { display:block; max-width:760px; margin:40px auto; color:var(--uui-color-text, #1d1d1f); }
    main { display:grid; gap:20px; } header, .digest-row, .actions { display:flex; align-items:center; justify-content:space-between; gap:12px; }
    h1, h2, p { margin:0; } h1 { font-size:28px; } h2 { font-size:18px; } header p, small { display:block; color:var(--uui-color-text-alt, #6b6b6b); margin-top:4px; }
    .digest-list { border-top:1px solid var(--uui-color-border, #d9d9d9); } .digest-row { width:100%; padding:16px 0; text-align:left; background:transparent; border:0; border-bottom:1px solid var(--uui-color-border, #d9d9d9); font:inherit; cursor:pointer; }
    .digest-row:hover strong, .back:hover { text-decoration:underline; } .status { color:#185c37; font-size:13px; } .status--paused { color:var(--uui-color-text-alt, #6b6b6b); }
    .empty { display:grid; gap:10px; padding:44px 0; text-align:center; } .empty uui-button { justify-self:center; margin-top:6px; }
    form, .advanced { display:grid; gap:16px; } label { display:grid; gap:6px; font-weight:600; } input, textarea, select { box-sizing:border-box; width:100%; padding:9px 10px; font:inherit; border:1px solid var(--uui-color-border, #b7b7b7); border-radius:4px; background:var(--uui-color-surface, #fff); }
    textarea { min-height:84px; resize:vertical; } .schedule, .grid { display:grid; grid-template-columns:repeat(3, minmax(0, 1fr)); gap:12px; } .grid { grid-template-columns:repeat(2, minmax(0, 1fr)); } .check { display:flex; align-items:center; gap:8px; } input[type=checkbox] { width:auto; }
    details { border-top:1px solid var(--uui-color-border, #d9d9d9); padding-top:16px; } summary { cursor:pointer; font-weight:600; } details > .advanced { margin-top:16px; } .package-settings { margin-top:12px; } .package-settings summary { display:flex; justify-content:space-between; } .package-settings summary span { color:var(--uui-color-text-alt, #6b6b6b); font-weight:400; }
    fieldset { border:0; padding:0; margin:0; } legend { font-weight:600; margin-bottom:10px; } .section-list { display:grid; grid-template-columns:repeat(2, minmax(0, 1fr)); gap:10px; } .back { padding:0; color:inherit; background:transparent; border:0; font:inherit; cursor:pointer; }
    [hidden] { display:none !important; } @media (max-width:600px) { :host { margin:24px 16px; } .schedule, .grid, .section-list { grid-template-columns:1fr; } }
`;

customElements.define("editorial-digest-settings", EditorialDigestSettings);
