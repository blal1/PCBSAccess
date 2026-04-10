# Phase 4: Web Browser and Services - Accessibility Design

**Date:** 2026-04-10
**Status:** Approved
**Scope:** All HtmlBrowser panels - web pages, bank, shop, hack shop, router config, search, jobs, police, CCTV, ISP, currency, CTF, device finder, router help

---

## Architecture

### Coordinator Pattern

**BrowserHandler** is the coordinator class registered in Main.cs. It:

1. Captures the HtmlBrowser instance via Harmony patch on `HtmlBrowser.Start()` (postfix)
2. Patches `ShowPanel(SubPanelWebs)` to detect panel changes (prefix - reads the parameter)
3. Maintains a `Dictionary<SubPanelWebs, ISubHandler>` mapping panel types to sub-handlers
4. On panel change: deactivates current sub-handler, activates new one, announces panel context
5. On browser window focus (via WindowFocusHandler integration): re-announces current panel
6. Routes keyboard input to the active sub-handler in `Update()`

### ISubHandler Interface

```csharp
interface ISubHandler
{
    void Activate(HtmlBrowser browser);
    void Deactivate();
    void HandleInput();
    void AnnounceState();
    string GetPanelName();
}
```

### Sub-Handler List

- WebPageSubHandler (SubPanelWebs.Web)
- BankSubHandler (BankLogin, BankRegister, BankProgram)
- ShopSubHandler (Shop)
- HackShopSubHandler (HackShopTools, HackShopExploits)
- RouterPortSubHandler (PanelPorts)
- RouterFirewallSubHandler (PanelFirewall)
- RouterHelpSubHandler (PanelHelp)
- SearchSubHandler (PanelSearch, PanelNoNet)
- JobsSubHandler (Jobs, PoliceJobs)
- PoliceSubHandler (PoliceReport)
- CCTVSubHandler (Cctv)
- ISPSubHandler (ISPConfig)
- CurrencySubHandler (CreateCurrency)
- CTFSubHandler (PanelCTF)
- FindDeviceSubHandler (PanelFindDeviceManual)

### BrowserPatches

Single Harmony patches class. All patches call static methods on BrowserHandler.

---

## Sub-Handler Designs

### WebPageSubHandler

**Handles:** Actual HTML pages rendered by PowerUI.

**On activation (page loaded):**
- Patches `ResumeConnectionWeb()` to detect when HTML content arrives
- Extracts page text from `panelCustom.Document.innerHTML` (strip HTML tags)
- Finds interactive buttons (elements with class "btn btn-primary"), builds navigable list
- Announces: "Web page. [N] links." plus trimmed text preview

**Navigation:**
- Up/Down: cycle through interactive buttons, announce button text
- Enter: trigger the button's click handler
- Alt+R: read page text content
- Space: repeat current button or page info

**Error pages:**
- Patches `CloseConnection()` to announce: "Page not found", "URL not found", or "No network access"

**Address bar:**
- Patches `EnterWeb()` postfix to announce "Loading [address]..."

**Search integration:**
- Patches `EnterSearch()` to announce "Searching..."
- Patches search results callback to announce result count
- Search results appear as web page links, handled naturally by this sub-handler

---

### BankSubHandler

**Handles:** BankLogin, BankRegister, BankProgram panels.

**Bank Login (SubPanelWebs.BankLogin):**
- Announces: "Bank login. Tab between fields, Enter to submit."
- Fields: bank_account, passwordLogin (TMP_InputField, Tab cycles natively)
- Patches `BankLogin()` postfix: "Logging in..."
- Patches `PlayerLoginBank()`: "Logged in. Balance: $[amount]"

**Bank Register (SubPanelWebs.BankRegister):**
- Announces: "Bank registration."
- Patches `CreateBankAccount()` postfix: "Creating account..."
- Patches `OnBankRegistration()`: success/failure with account number

**Bank Account (SubPanelWebs.BankProgram):**
- Announces: "Bank account. Balance: $[amount]. [N] transactions."
- Up/Down: navigate transaction list (bankListAdapter), announce date, amount, description per entry
- Tab: cycle to transfer fields (destination account, amount)
- Enter on transfer: submit, announce confirmation
- Space: repeat balance

**Shared:** Form validation errors announced via panelWebHelper error text detection.

---

### ShopSubHandler

**Handles:** Regular software/hardware shop (SubPanelWebs.Shop).

**On activation:**
- Patches `ResumeConnectionWindowFilesShop()` to detect item load
- Announces: "Shop. [N] items. Up Down to browse, Enter to buy."
- Reads items from `listaItems` (software) or `listaItemsHw` (hardware)

**Navigation:**
- Up/Down: browse items, announce name, description, price
- Enter: triggers `OnBuy()`, opens PreBuy dialog
- Alt+F: cycle filter dropdown, announce current filter
- Space: repeat current item details

---

### HackShopSubHandler

**Handles:** HackShopTools and HackShopExploits panels.

**Tools (SubPanelWebs.HackShopTools):**
- Announces: "Hack shop tools. [N] items. Up Down to browse."
- Same Up/Down/Enter/Space pattern as regular shop

**Exploits (SubPanelWebs.HackShopExploits):**
- Announces: "Hack shop exploits. Select library and version to search."
- Alt+F: cycle library dropdown
- After search: "Found [N] exploits. Up Down to browse."
- Up/Down: browse exploits, announce name, service affected, price, description
- Enter: opens PreBuy dialog
- Alt+P: cycle permission filter dropdown

**PreBuy dialog (handled by BrowserHandler directly, not a sub-handler):**
- BrowserHandler patches PreBuy/PreBuyHardware `Configure()` to detect when purchase dialog opens
- Temporarily suspends active sub-handler input routing while PreBuy is open
- Announces: "Purchase: [item name]. Price: $[amount] ([N] coupons). Version: [version]."
- Up/Down or Alt+V: cycle version dropdown if multiple versions
- Tab: toggle source code checkbox (hack shop only), announce checked/unchecked
- Enter: confirm purchase
- Escape: cancel
- On close: resumes active sub-handler

---

### RouterPortSubHandler

**Handles:** Port forwarding (SubPanelWebs.PanelPorts).

**On activation:**
- Patches `ResumeRouterConfig()` to detect data load
- Announces: "Port forwarding. [N] rules. Up Down to browse, Enter to edit, N to add."

**Browse mode:**
- Up/Down: navigate rules, announce "Rule [X] of [N]: External [port] to [LAN IP]:[internal port]" plus "Protected" if locked
- Enter: enter edit mode (unless protected)
- N: add new rule, enter edit mode on blank rule
- Delete: delete selected rules
- Space: repeat current rule

**Edit mode:**
- Announces: "Editing rule. Tab between fields, Enter to save, Escape to cancel."
- Tab: cycle external port, internal port, LAN IP - announce field name and current value
- Enter: save (validates, announces success or error)
- Escape: cancel, return to browse mode

---

### RouterFirewallSubHandler

**Handles:** Firewall rules (SubPanelWebs.PanelFirewall).

**On activation:**
- Announces: "Firewall rules. [N] rules. Up Down to browse, Enter to edit, N to add."

**Browse mode:**
- Up/Down: navigate rules, announce "Rule [X] of [N]: [Allow/Deny] port [port] from [source] to [dest]" with "Any" where applicable
- Enter/N/Delete/Space: same pattern as port forwarding

**Edit mode:**
- Tab: cycle action, port, source IP, destination IP
- On action field: Left/Right to toggle Allow/Deny
- On port/source/dest: type value, or Alt+A to toggle "Any"
- Enter to save, Escape to cancel

---

### RouterHelpSubHandler

**Handles:** Router help page (SubPanelWebs.PanelHelp).

- Announces: "Router help."
- Alt+R: read full help text content

---

### SearchSubHandler

**Handles:** PanelSearch and PanelNoNet.

- PanelSearch: announces "Search. Type query in address bar, Enter to search." Focus to address bar.
- PanelNoNet: announces "No network connection."
- Search results handled by WebPageSubHandler (results are web page links).

---

### JobsSubHandler

**Handles:** Jobs and PoliceJobs panels.

**On activation:**
- Announces: "Jobs. [N] missions available. Up Down to browse." (or "Police jobs.")
- Items are ItemJobs components

**Navigation:**
- Up/Down: browse jobs, announce title and reward
- Enter: select job, patches `OnMissionClick()` to announce mission details (description, objectives, cooldown)
- Space: repeat current job
- Backspace: back to job list from detail view

---

### PoliceSubHandler

**Handles:** Police report filing (SubPanelWebs.PoliceReport).

**On activation:**
- Announces: "Police report. Enter IP address and attach evidence."

**Navigation:**
- Tab: cycle between IP input and attach button
- Enter on attach: triggers file dialog
- Enter on submit: patches `OnSendAttach()`, announces success or validation error
- Space: repeat current field info

---

### CCTVSubHandler

**Handles:** CCTV panel (SubPanelWebs.Cctv).

- On activation: announces "CCTV camera. W A S D to pan camera." Plus camera name/ID from BrowserCam if available.
- Password prompt: announces "Enter camera password." when `OnEnterCam` is called with password field.
- No image description - visual limitation acknowledged.

---

### ISPSubHandler

**Handles:** ISP configuration (SubPanelWebs.ISPConfig).

- Announces panel name and available options
- Up/Down to navigate settings, Enter to interact
- Reads field labels and values from panel TMP_Text children

---

### CurrencySubHandler

**Handles:** Cryptocurrency creation (SubPanelWebs.CreateCurrency).

- Announce panel, navigate fields, read labels/values
- Same announce-navigate-interact pattern

---

### CTFSubHandler

**Handles:** CTF events (SubPanelWebs.PanelCTF).

- Announce panel, navigate available events, read details

---

### FindDeviceSubHandler

**Handles:** Device manual finder (SubPanelWebs.PanelFindDeviceManual).

- Announce panel, input fields for search, results navigation

---

## Key Bindings

### Browser-Level Keys (BrowserHandler)

- Alt+Left: back in history
- Alt+Right: forward in history
- Alt+Home: go to search home page
- Alt+R: read current content (delegates to sub-handler)
- F1: browser help text
- Space: repeat current state (delegates to sub-handler)

### Panel-Specific Keys (routed to active sub-handler)

- Up/Down: navigate items/rules/links
- Enter: activate/select/submit
- Escape: cancel edit mode or close sub-panel
- Tab: cycle form fields
- N: add new entry (router config panels)
- Delete: delete selected (router config panels)
- Backspace: go back from detail views
- Alt+F: cycle filter dropdowns (shop panels)
- Alt+P: cycle permission filter (hack shop exploits)
- Alt+V: cycle version dropdown (PreBuy dialog)
- Alt+A: toggle "Any" on router firewall fields
- Left/Right: toggle Allow/Deny (firewall edit mode action field)

### Announcement Patterns

- Panel entry: "[Panel name]. [count] items. [navigation hint]."
- Item navigation: "[Position] of [total]: [item details]"
- Action feedback: "[Action]. [Result]."
- Errors: "[Error description]."
- Mode changes: "Editing [thing]. [available actions]."

---

## Harmony Patches (BrowserPatches.cs)

- `HtmlBrowser.Start()` - capture instance (postfix)
- `HtmlBrowser.ShowPanel(SubPanelWebs)` - panel change detection (prefix)
- `HtmlBrowser.ResumeConnectionWeb()` - HTML page loaded (postfix)
- `HtmlBrowser.CloseConnection()` - error page (postfix)
- `HtmlBrowser.EnterWeb()` - navigation started (postfix)
- `HtmlBrowser.PlayerLoginBank()` - bank login result (postfix)
- `HtmlBrowser.OnBankRegistration()` - bank registration result (postfix)
- `HtmlBrowser.CreateBankAccount()` - bank registration submitted (postfix)
- `HtmlBrowser.ResumeConnectionWindowFilesShop()` - shop items loaded (postfix)
- `HtmlBrowser.ResumeRouterConfig()` - router data loaded (postfix)
- `HtmlBrowser.OnMissionClick()` - job selected (postfix)
- `HtmlBrowser.OnSendAttach()` - police report submitted (postfix)
- `HtmlBrowser.EnterSearch()` - search initiated (postfix)
- `PreBuy.Configure()` - purchase dialog opened (postfix)
- `PreBuyHardware.Configure()` - hardware purchase dialog opened (postfix)

---

## File Structure

```
BrowserHandler.cs          — Coordinator, patches routing, shared state
BrowserPatches.cs          — All Harmony patches for HtmlBrowser
ISubHandler.cs             — Interface definition
WebPageSubHandler.cs       — HTML web pages
BankSubHandler.cs          — Bank login/register/account
ShopSubHandler.cs          — Regular shop
HackShopSubHandler.cs      — Hack shop tools/exploits + PreBuy
RouterPortSubHandler.cs    — Port forwarding
RouterFirewallSubHandler.cs — Firewall rules
RouterHelpSubHandler.cs    — Router help text
SearchSubHandler.cs        — Search home + no network
JobsSubHandler.cs          — Jobs + police jobs
PoliceSubHandler.cs        — Police report filing
CCTVSubHandler.cs          — CCTV camera
ISPSubHandler.cs           — ISP configuration
CurrencySubHandler.cs      — Cryptocurrency creation
CTFSubHandler.cs           — CTF events
FindDeviceSubHandler.cs    — Device manual finder
```

Total: 18 files. Each sub-handler expected to be 30-150 lines. BrowserHandler and BrowserPatches will be the largest (200-300 lines each).

---

## Notes

- ISP, Currency, CTF, FindDevice, and RouterHelp are simpler panels. Their exact UI elements will be analyzed during implementation with the same announce-navigate-interact pattern.
- PreBuy dialog handling lives in BrowserHandler directly (not a sub-handler), since it applies to both regular and hack shop purchases.
- Auto-activation on browser window focus, consistent with file explorer and mail patterns.
- All strings go through Loc.Get() for localization.
