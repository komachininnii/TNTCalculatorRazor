// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ヘルプウィンドウを開く
function openHelpWindow(e, url) {
    e.preventDefault(); // 既定遷移（=二重オープン）を止める

    var name = "TNT_Help";
    var features = "width=900,height=800,resizable=yes,scrollbars=yes";

    var w = window.open(url, name, features);

    // ポップアップがブロックされたら、新しいタブで開く（フォールバック）
    if (!w) {
        window.open(url, "_blank", "noopener");
        return;
    }

    try { w.focus(); } catch (e) { }
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

(function () {
    "use strict";

    // Enter直後のblur二重送信を防ぐ（現状Index.cshtmlと同じ目的）
    var tntSkipNextBlurSubmit = false;

    function getForm(target) {
        return (target && target.form) || document.querySelector("form[data-tnt-form]") || document.querySelector("form");
    }

    function setAction(action) {
        var el = document.getElementById("Action");
        if (el) el.value = action;
    }

    // 多重送信ガード（blur連打や change+blur 連続など）
    function submitGuarded(form) {
        if (!form) return;
        if (form.__tntSubmitting) return;
        form.__tntSubmitting = true;
        try { form.submit(); }
        finally { window.setTimeout(function () { form.__tntSubmitting = false; }, 300); }
    }

    function trim(v) { return v ? v.replace(/^\s+|\s+$/g, "") : ""; }

    // ==== 1) oninput不要：data-maxint/maxdec が付いてる要素に tntLimitNumber を自動適用 ====
    document.addEventListener("input", function (e) {
        e = e || window.event;
        var t = e.target || e.srcElement;
        if (!t || !t.getAttribute) return;
        if (!t.getAttribute("data-maxint") && !t.getAttribute("data-maxdec")) return;

        // 既存の tntLimitNumber をそのまま利用
        tntLimitNumber(t);
    });

    // ==== 2) Enter送信：data-enter-action ====
    document.addEventListener("keydown", function (e) {
        e = e || window.event;
        var t = e.target || e.srcElement;
        if (!t || !t.getAttribute) return;

        var action = t.getAttribute("data-enter-action");
        if (!action) return;

        // input以外は無視（textarea/select等でEnterを奪わない）
        var tag = (t.tagName || "").toLowerCase();
        if (tag !== "input") return;

        var code = e.keyCode || e.which;
        if (code !== 13) return;

        if (e.preventDefault) e.preventDefault();
        e.returnValue = false;

        tntSkipNextBlurSubmit = true;

        setAction(action);
        submitGuarded(getForm(t));
        return false;
    });


    // ==== 3) change送信：data-change-action（energy/volume/renal等）====
    document.addEventListener("change", function (e) {
        var t = e.target || e.srcElement;
        if (!t || !t.getAttribute) return;

        var action = t.getAttribute("data-change-action");
        if (!action) return;

        setAction(action);
        submitGuarded(getForm(t));
    });

    // ==== 4) blur送信：smart blur か、単純blur(action固定)のどちらか ====
    function smartBlurSubmit(form) {
        if (tntSkipNextBlurSubmit) {
            tntSkipNextBlurSubmit = false;
            return;
        }

        var h = document.getElementById("Height");
        var w = document.getElementById("Weight");
        var cr = document.getElementById("SerumCreatinine");
        var act = document.getElementById("Action");

        var heightStr = trim(h && h.value);
        var weightStr = trim(w && w.value);
        var crStr = trim(cr && cr.value);

        var doRenal = (weightStr !== "" && crStr !== "");
        var doAnthro = (weightStr !== "" && heightStr !== "");

        if (!doRenal && !doAnthro) return;

        // 入力継続中なら送信しない（現状の tntSubmitOnBlurSmart と同じ）
        window.setTimeout(function () {
            var ae = document.activeElement;
            var id = (ae && ae.id) ? ae.id : "";

            if (id === "Age" || id === "Height" || id === "Weight" || id === "SerumCreatinine") return;

            if (act) act.value = doRenal ? "renal" : "anthro";
            submitGuarded(form);
        }, 0);
    }

    // blurはバブリングしないので focusout を使う
    document.addEventListener("focusout", function (e) {
        var t = e.target || e.srcElement;
        if (!t || !t.getAttribute) return;

        // 4-A) smart blur 対象
        if (t.getAttribute("data-smart-blur") === "1") {
            smartBlurSubmit(getForm(t));
            return;
        }

        // 4-B) 単純 blur(action固定) を使いたい場合（必要ならHTMLに data-blur-action を付ける）
        var blurAction = t.getAttribute("data-blur-action");
        if (blurAction) {
            if (tntSkipNextBlurSubmit) {
                tntSkipNextBlurSubmit = false;
                return;
            }
            setAction(blurAction);
            submitGuarded(getForm(t));
        } else {
            // 既存仕様：blur時、Actionが空ならcalcセットだけ（送信しない）
            var act = document.getElementById("Action");
            if (act && !act.value) act.value = "calc";
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
    });

})();

