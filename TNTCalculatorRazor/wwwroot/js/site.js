// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ヘルプウィンドウを開く
function openHelpWindow(e, url) {
    e.preventDefault();

    var name = "TNT_Help";
    var features = "width=760,height=800,resizable=yes,scrollbars=yes";
    var w = window.open(url, name, features);

    if (!w) {
        // ブロック時フォールバック（余計な指定をしない）
        window.open(url, "_blank");
        return;
    }

    // opener遮断（IE11でもOK。できない環境ではtry/catchで無害）
    try { w.opener = null; } catch (err) { }

    try { w.focus(); } catch (err) { }
}

// data-help-window をクリックしたら openHelpWindow で開く（フッター/本文どこでもOK）
document.addEventListener("click", function (e) {
    e = e || window.event;
    var t = e.target || e.srcElement;

    // 親を辿って data-help-window を探す（IE11対応：closest不使用）
    while (t && t !== document) {
        if (t.getAttribute && t.getAttribute("data-help-window") !== null) break;
        t = t.parentNode;
    }
    if (!t || t === document) return;

    var url = t.getAttribute("href");
    if (!url) return;

    openHelpWindow(e, url);
});

// Privacyウィンドウを開く（ヘルプと同様）
function openPrivacyWindow(e, url) {
    e.preventDefault();

    var name = "TNT_Privacy";
    var features = "width=760,height=800,resizable=yes,scrollbars=yes";
    var w = window.open(url, name, features);

    if (!w) {
        // ブロック時フォールバック（余計な指定をしない）
        window.open(url, "_blank");
        return;
    }

    // opener遮断（IE11でもOK。できない環境ではtry/catchで無害）
    try { w.opener = null; } catch (err) { }

    try { w.focus(); } catch (err) { }
}

// data-privacy-window をクリックしたら openPrivacyWindow で開く
document.addEventListener("click", function (e) {
    e = e || window.event;
    var t = e.target || e.srcElement;

    // 親を辿って data-privacy-window を探す（IE11対応：closest不使用）
    while (t && t !== document) {
        if (t.getAttribute && t.getAttribute("data-privacy-window") !== null) break;
        t = t.parentNode;
    }
    if (!t || t === document) return;

    var url = t.getAttribute("href");
    if (!url) return;

    openPrivacyWindow(e, url);
});



// 数値入力を「桁数/小数桁」で物理的に制限する（IE11対応版）
// 使い方: data-maxint / data-maxdec / data-sign を input に付ける（oninput不要）
function tntLimitNumber(el) {
    var maxInt = parseInt(el.getAttribute('data-maxint') || '', 10);
    var maxDec = parseInt(el.getAttribute('data-maxdec') || '', 10);
    var allowSign = (el.getAttribute('data-sign') === '1');

    var raw = (el.value || '').toString();

    // IE11対応：末尾 '.' 判定
    var keepTrailingDot = (raw.length > 0 && raw.charAt(raw.length - 1) === '.');

    // 1) 先頭符号
    var sign = '';
    if (allowSign && raw.charAt(0) === '-') {
        sign = '-';
        raw = raw.slice(1);
    }

    // 2) 数字と小数点以外を除去
    var v = raw.replace(/[^\d.]/g, '');

    // 3) 小数点は最初の1個だけ残す
    var firstDot = v.indexOf('.');
    if (firstDot !== -1) {
        v = v.slice(0, firstDot + 1) + v.slice(firstDot + 1).replace(/\./g, '');
    }

    // 4) IE11対応：先頭 '.' は '0.' に
    if (v.length > 0 && v.charAt(0) === '.') {
        v = '0' + v;
        firstDot = v.indexOf('.');
    }

    // 5) 整数部/小数部
    var parts = v.split('.');
    var intPart = parts[0] || '';
    var decPart = (parts.length > 1) ? (parts[1] || '') : '';

    // 6) 先頭ゼロ整理（0.xxxは残す）
    if (intPart !== '0') {
        intPart = intPart.replace(/^0+(?=\d)/, '');
    }

    // 7) 桁制限
    if (!isNaN(maxInt) && maxInt > 0 && intPart.length > maxInt) {
        intPart = intPart.slice(0, maxInt);
    }
    if (!isNaN(maxDec) && maxDec >= 0) {
        if (maxDec === 0) {
            decPart = '';
            keepTrailingDot = false;
        } else if (decPart.length > maxDec) {
            decPart = decPart.slice(0, maxDec);
        }
    }

    // 8) 再構成
    var out = sign + intPart;
    if (firstDot !== -1 || keepTrailingDot) {
        out += '.';
        out += decPart;
    }

    // 9) 値更新＋caret末尾
    if (el.value !== out) {
        el.value = out;
        try { el.setSelectionRange(out.length, out.length); } catch (e) { }
    }
}


// ==== フォーム自動送信共通処理：数値制限、Enter送信、change送信、blur送信 ====
(function () {
    "use strict";

    // Enter直後のblur二重送信を防ぐ
    var tntSkipNextBlurSubmit = false;

    function getForm(target) {
        return (target && target.form) || document.querySelector("form[data-tnt-form]") || document.querySelector("form");
    }

    // 多重送信ガード
    function submitGuarded(form) {
        if (!form) return;
        if (form.__tntSubmitting) return;
        form.__tntSubmitting = true;
        try { form.submit(); }
        finally { window.setTimeout(function () { form.__tntSubmitting = false; }, 300); }
    }

    function canUseAjax() {
        return typeof window.fetch === "function" && typeof window.FormData !== "undefined";
    }

    function buildRecalcUrl(form) {
        var action = (form && form.getAttribute) ? form.getAttribute("action") : "";
        if (!action) action = window.location.href;
        return action + (action.indexOf("?") >= 0 ? "&" : "?") + "handler=Recalc";
    }

    var tntLastLayoutIsMobile = null;

    function getIsMobileLayout() {
        if (window.matchMedia) {
            return window.matchMedia("(max-width: 980px)").matches;
        }

        var w = document.documentElement.clientWidth || document.body.clientWidth;
        return (w <= 980);
    }

    function setResultDetailsOpenByLayout() {
        var isMobile = getIsMobileLayout();
        tntLastLayoutIsMobile = isMobile;

        var list = document.querySelectorAll(".enteral-details, .result-details");
        for (var i = 0; i < list.length; i++) {
            var d = list[i];
            if (!d) continue;
            if (isMobile) {
                if (d.removeAttribute) d.removeAttribute("open");
                if ("open" in d) d.open = false;
            } else {
                if (d.setAttribute) d.setAttribute("open", "open");
                if ("open" in d) d.open = true;
            }
        }
    }

    window.tntSetResultDetailsOpenByLayout = setResultDetailsOpenByLayout;

    function readJsonScript(dataEl) {
        if (!dataEl) return "";
        return dataEl.textContent || dataEl.innerText || dataEl.text || dataEl.innerHTML || "";
    }

    // IE対策：<script type="application/json">の中身が取得できない場合があるため、
    // scriptの内容が空なら data-* 属性のJSONを使う。
    function readPanelJson(panel, scriptId, dataAttr) {
        var scriptEl = panel ? panel.querySelector(scriptId) : null;
        var raw = scriptEl ? readJsonScript(scriptEl) : "";
        if (raw && raw.replace(/\s+/g, "") !== "") return raw;

        var dataEl = panel ? panel.querySelector("#resultPanelData") : null;
        if (!dataEl) return "{}";

        var attr = dataEl.getAttribute(dataAttr);
        return attr || "{}";
    }

    // 再計算結果のエラーJSONを左カラムの入力欄へ反映する。
    // IEでは classList が使えない場合があるため className 操作も併用する。
    function applyResultErrorsFromPanel(panel) {
        if (!panel) return;

        var data;
        try {
            data = JSON.parse(readPanelJson(panel, "#resultPanelErrorData", "data-errors"));
        } catch (e) {
            return;
        }

        for (var key in data) {
            if (!Object.prototype.hasOwnProperty.call(data, key)) continue;
            var msg = data[key] || "";
            var container = document.querySelector('[data-error-for="' + key + '"]');
            if (!container) continue;

            var span = container.querySelector("span");
            if (!span) {
                span = document.createElement("span");
                container.appendChild(span);
            }

            if (msg) {
                span.textContent = msg;
                if (container.classList && container.classList.add) {
                    container.classList.add("field-error--visible");
                } else {
                    container.className = (container.className || "") + " field-error--visible";
                }
                container.style.display = "block";
            } else {
                span.textContent = "";
                if (container.classList && container.classList.remove) {
                    container.classList.remove("field-error--visible");
                } else {
                    container.className = (container.className || "").replace(/\bfield-error--visible\b/g, "");
                }
                container.style.display = "none";
            }
        }
    }

    // 必要エネルギー関連（算出方法/数値/候補）を左カラムへ同期する。
    function applyEnergyFromPanel(panel) {
        if (!panel) return;

        var data;
        try {
            data = JSON.parse(readPanelJson(panel, "#resultPanelEnergyData", "data-energy"));
        } catch (e) {
            return;
        }

        var select = document.querySelector("[data-energy-select]");
        if (select && data.SelectedEnergyOrder !== undefined && data.SelectedEnergyOrder !== null) {
            select.value = data.SelectedEnergyOrder;
        }

        var input = document.querySelector("[data-energy-input]");
        if (input) {
            if (data.EnergyOrderValue === null || data.EnergyOrderValue === undefined) {
                input.value = "";
            } else {
                input.value = data.EnergyOrderValue;
            }
        }

        var pill = document.querySelector("[data-energy-user-edited]");
        if (pill) {
            var hasEnergyValue = data.EnergyOrderValue !== null && data.EnergyOrderValue !== undefined && data.EnergyOrderValue !== "";
            pill.style.display = (data.IsEnergyUserEdited && hasEnergyValue) ? "" : "none";
        }

        var keys = ["EnergyByBmrKcal", "Kcal25", "Kcal30", "Kcal35"];
        for (var i = 0; i < keys.length; i++) {
            var key = keys[i];
            var el = document.querySelector('[data-energy-candidate="' + key + '"]');
            if (!el) continue;
            var value = data[key];
            el.textContent = (value === null || value === undefined || value === "") ? "-" : value;
        }
    }

    // 疾患・妊娠・肝性脳症・蛋白補正など条件付きUIを同期する。
    function applyFormStateFromPanel(panel) {
        if (!panel) return;

        var data;
        try {
            data = JSON.parse(readPanelJson(panel, "#resultPanelFormStateData", "data-form-state"));
        } catch (e) {
            return;
        }

        var diseaseWrapper = document.querySelector("[data-disease-wrapper]");
        if (diseaseWrapper) {
            diseaseWrapper.style.display = data.ShowDisease ? "" : "none";
        }

        var diseaseSelect = document.querySelector("[data-disease-select]");
        if (diseaseSelect && data.SelectedDisease !== undefined && data.SelectedDisease !== null) {
            diseaseSelect.value = data.SelectedDisease;
        }

        var pregnantWrapper = document.querySelector("[data-pregnant-wrapper]");
        if (pregnantWrapper) {
            pregnantWrapper.style.display = data.ShowPregnant ? "" : "none";
        }

        var pregnantInput = document.querySelector("[data-pregnant-input]");
        if (pregnantInput) {
            pregnantInput.checked = !!data.IsPregnant;
        }

        var hepaticWrapper = document.querySelector("[data-hepatic-wrapper]");
        if (hepaticWrapper) {
            hepaticWrapper.style.display = data.ShowHepatic ? "" : "none";
        }

        var hepaticInput = document.querySelector("[data-hepatic-input]");
        if (hepaticInput) {
            hepaticInput.checked = data.ShowHepatic ? !!data.IsHepaticEncephalopathy : false;
        }

        var proteinSelect = document.querySelector("[data-protein-select]");
        if (proteinSelect && data.SelectedProteinCorrection !== undefined && data.SelectedProteinCorrection !== null) {
            proteinSelect.value = data.SelectedProteinCorrection;
        }

        var proteinFlag = document.querySelector('input[name="IsProteinCorrectionUserEdited"]');
        if (proteinFlag && data.IsProteinCorrectionUserEdited !== undefined && data.IsProteinCorrectionUserEdited !== null) {
            proteinFlag.value = data.IsProteinCorrectionUserEdited ? "true" : "false";
        }
    }

    window.tntApplyFormStateFromPanel = applyFormStateFromPanel;

    // Enter/blur/changeの自動計算をAJAXで処理し、結果パネルのみ差し替える。
    // fetch非対応（IE等）の場合は従来submitへフォールバックする。
    function submitWithRecalc(form) {
        if (!form) return;
        if (!canUseAjax()) {
            submitGuarded(form);
            return;
        }
        if (form.__tntSubmitting) return;
        form.__tntSubmitting = true;

        var fd = new FormData(form);
        var url = buildRecalcUrl(form);

        fetch(url, {
            method: "POST",
            body: fd,
            credentials: "same-origin"
        })
            .then(function (r) { return r.text(); })
            .then(function (html) {
                var panel = document.getElementById("resultPanel");
                if (panel) {
                    panel.innerHTML = html;
                    setResultDetailsOpenByLayout();
                    applyResultErrorsFromPanel(panel);
                    applyEnergyFromPanel(panel);
                    applyFormStateFromPanel(panel);
                }
            })
            .catch(function () {
                form.__tntSubmitting = false;
                submitGuarded(form);
            })
            .then(function () {
                if (form.__tntSubmitting) {
                    window.setTimeout(function () { form.__tntSubmitting = false; }, 300);
                }
            });
    }

    function trim(v) { return v ? v.replace(/^\s+|\s+$/g, "") : ""; }

    // ==== 1) 数値制限：data-maxint/maxdec属性を持つ要素にtntLimitNumberを適用 ====
    document.addEventListener("input", function (e) {
        e = e || window.event;
        var t = e.target || e.srcElement;
        if (!t || !t.getAttribute) return;
        if (!t.getAttribute("data-maxint") && !t.getAttribute("data-maxdec")) return;
        tntLimitNumber(t);
    });

    // ==== 2) Enter送信：data-enter-action ====
    document.addEventListener("keydown", function (e) {
        e = e || window.event;
        var t = e.target || e.srcElement;
        if (!t || !t.getAttribute) return;

        var action = t.getAttribute("data-enter-action");
        if (!action) return;

        var tag = (t.tagName || "").toLowerCase();
        if (tag !== "input") return;

        var code = e.keyCode || e.which;
        if (code !== 13) return;

        if (e.preventDefault) e.preventDefault();
        e.returnValue = false;

        tntSkipNextBlurSubmit = true;

        var form = getForm(t);
        if (form) {
            var actionField = form.querySelector('input[name="Action"]');
            if (actionField) actionField.value = action;
            submitWithRecalc(form);
        }
        return false;
    });

    // ==== 3) change送信：data-change-action（全てのselectやcheckbox対象）====
    document.addEventListener("change", function (e) {
        var t = e.target || e.srcElement;
        if (!t || !t.getAttribute) return;

        var action = t.getAttribute("data-change-action");
        if (!action) return;

        var form = getForm(t);
        if (!form) return;

        // ★蛋白補正セレクトは特殊処理
        if (t.name === "SelectedProteinCorrection") {
            var flag = form.querySelector('input[name="IsProteinCorrectionUserEdited"]');
            if (flag) {
                flag.value = (t.value && t.value !== "None") ? "true" : "false";
            }
        }

        var actionField = form.querySelector('input[name="Action"]');
        if (actionField) actionField.value = action;

        submitWithRecalc(form);
    });

    // ==== 4) blur送信：smart blur または data-blur-action ====
    function smartBlurSubmit(form) {
        if (tntSkipNextBlurSubmit) {
            tntSkipNextBlurSubmit = false;
            return;
        }

        var h = document.getElementById("Height");
        var w = document.getElementById("Weight");
        var cr = document.getElementById("SerumCreatinine");
        var actField = form ? form.querySelector('input[name="Action"]') : null;

        var heightStr = trim(h && h.value);
        var weightStr = trim(w && w.value);
        var crStr = trim(cr && cr.value);

        var doRenal = (weightStr !== "" && crStr !== "");
        var doAnthro = (weightStr !== "" && heightStr !== "");

        if (!doRenal && !doAnthro) return;

        // 入力継続中なら送信しない
        window.setTimeout(function () {
            var ae = document.activeElement;
            var id = (ae && ae.id) ? ae.id : "";

            if (id === "Age" || id === "Height" || id === "Weight" || id === "SerumCreatinine") return;

            if (actField) actField.value = doRenal ? "renal" : "anthro";
            submitWithRecalc(form);
        }, 0);
    }

    // blurはバブリングしないので focusout を使う
    document.addEventListener("focusout", function (e) {
        var t = e.target || e.srcElement;
        if (!t || !t.getAttribute) return;

        var form = getForm(t);
        if (!form) return;

        // 4-A) smart blur
        if (t.getAttribute("data-smart-blur") === "1") {
            smartBlurSubmit(form);
            return;
        }

        // 4-B) 単純 blur
        var blurAction = t.getAttribute("data-blur-action");
        var actField = form.querySelector('input[name="Action"]');

        if (blurAction) {
            if (tntSkipNextBlurSubmit) {
                tntSkipNextBlurSubmit = false;
                return;
            }
            if (actField) actField.value = blurAction;
            submitWithRecalc(form);
        } else {
            if (actField && !actField.value) actField.value = "calc";
        }
    });

})();



// ==== details要素のIE11対応と、モバイルでの初期折りたたみ＋ホーム画面追加案内 ====
(function () {
    "use strict";

    function tntInitMobileDetailsAndHint() {
        var isMobile = false;

        if (window.matchMedia) {
            isMobile = window.matchMedia("(max-width: 980px)").matches;
        } else {
            var w = document.documentElement.clientWidth || document.body.clientWidth;
            isMobile = (w <= 980);
        }

        if (!isMobile) return;

        // スマホでは result/enteral の open を外す（初期で折りたたむ）
        var list = document.querySelectorAll(".enteral-details, .result-details");
        for (var i = 0; i < list.length; i++) {
            list[i].removeAttribute("open");
        }

        // 初回だけホーム画面追加の案内
        var key = "tnt_homehint_dismissed_v1";
        try {
            if (window.localStorage && localStorage.getItem(key) === "1") return;
        } catch (e) { }

        var el = document.getElementById("homehint");
        if (!el) return;

        el.style.display = "flex";

        var btn = el.querySelector(".homehint-close");
        if (btn) {
            btn.onclick = function () {
                try { if (window.localStorage) localStorage.setItem(key, "1"); } catch (e2) { }
                if (el && el.parentNode) el.parentNode.removeChild(el);
                return false;
            };
        }
    }

    function tntInitDetailsPolyfillForIE() {
        var test = document.createElement("details");
        if ("open" in test) return; // 対応ブラウザは何もしない

        // ★ fold だけ対象にする（将来の不可視事故を防ぐ）
        var details = document.querySelectorAll ? document.querySelectorAll("details.fold")
            : document.getElementsByTagName("details");

        for (var i = 0; i < details.length; i++) {
            (function (d) {
                if (!document.querySelectorAll) {
                    var c = " " + (d.className || "") + " ";
                    if (c.indexOf(" fold ") < 0) return;
                }

                var summary = d.getElementsByTagName("summary")[0];
                if (!summary) return;

                function apply(open) {
                    if (open) d.setAttribute("open", "open");
                    else d.removeAttribute("open");

                    for (var j = 0; j < d.children.length; j++) {
                        var child = d.children[j];
                        if (child.tagName && child.tagName.toLowerCase() === "summary") continue;
                        child.style.display = open ? "" : "none";
                    }
                }

                var isOpen = (d.getAttribute("open") !== null);
                apply(isOpen);

                summary.onclick = function (e) {
                    if (e && e.preventDefault) e.preventDefault();
                    isOpen = !isOpen;
                    apply(isOpen);
                    return false;
                };
            })(details[i]);
        }
    }
    // DOMが揃ってから、順番固定で呼ぶ
    document.addEventListener("DOMContentLoaded", function () {
        // 1) まずモバイルなら open を外す
        tntInitMobileDetailsAndHint();
        // 2) 次に IE11なら “現在のopen属性状態” を表示に反映し、summaryで開閉できるようにする
        tntInitDetailsPolyfillForIE();
        // 3) 最後に tnt-ready クラスを追加（CSSで非表示解除に使う）
        var root = document.documentElement;

        if (root.classList && root.classList.add) {
            root.classList.add("tnt-ready");
        } else {
            // fallback: スペースと重複を安全に扱う
            var cn = root.className || "";
            if ((" " + cn + " ").indexOf(" tnt-ready ") === -1) {
                root.className = cn ? (cn + " tnt-ready") : "tnt-ready";
            }
        }

        if (window.tntSetResultDetailsOpenByLayout) {
            window.tntSetResultDetailsOpenByLayout();
        }

        if (window.tntApplyFormStateFromPanel) {
            var panel = document.getElementById("resultPanel");
            if (panel) {
                window.tntApplyFormStateFromPanel(panel);
            }
        }

        window.addEventListener("resize", function () {
            var isMobile = getIsMobileLayout();
            if (tntLastLayoutIsMobile !== isMobile && window.tntSetResultDetailsOpenByLayout) {
                window.tntSetResultDetailsOpenByLayout();
            }
        });
    });

})();
