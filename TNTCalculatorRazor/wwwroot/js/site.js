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

// 数値入力を「桁数/小数桁」で物理的に制限する
// - maxInt: 整数部の最大桁数（例: 年齢3桁）
// - maxDec: 小数部の最大桁数（例: 身長/体重1桁, Cr 2桁）
// - allowSign: trueなら先頭の '-' を許可（通常はfalse）
// 使い方: oninput="tntLimitNumber(this)"
function tntLimitNumber(el) {
    var maxInt = parseInt(el.getAttribute('data-maxint') || '', 10);
    var maxDec = parseInt(el.getAttribute('data-maxdec') || '', 10);
    var allowSign = (el.getAttribute('data-sign') === '1');

    var v = (el.value || '').toString();

    // 1) 先頭符号
    var sign = '';
    if (allowSign && v.charAt(0) === '-') {
        sign = '-';
        v = v.slice(1);
    }

    // 2) 数字と小数点以外を除去（e/E/+/- も消す）
    v = v.replace(/[^\d.]/g, '');

    // 3) 小数点は最初の1個だけ残す
    var firstDot = v.indexOf('.');
    if (firstDot !== -1) {
        v = v.slice(0, firstDot + 1) + v.slice(firstDot + 1).replace(/\./g, '');
    }

    // 4) 整数部/小数部に分割
    var parts = v.split('.');
    var intPart = parts[0] || '';
    var decPart = (parts.length > 1) ? (parts[1] || '') : '';

    // 5) 先頭ゼロは「0」「0.xxx」以外は削る（任意だが入力が安定）
    // 例: 00012 -> 12
    intPart = intPart.replace(/^0+(?=\d)/, '');

    // 6) 桁制限
    if (!isNaN(maxInt) && maxInt > 0 && intPart.length > maxInt) {
        intPart = intPart.slice(0, maxInt);
    }
    if (!isNaN(maxDec) && maxDec >= 0) {
        if (maxDec === 0) {
            decPart = '';
        } else if (decPart.length > maxDec) {
            decPart = decPart.slice(0, maxDec);
        }
    }

    // 7) 再構成（末尾の '.' は許す：入力中のストレス軽減）
    var out = sign + intPart;
    if (firstDot !== -1) {
        out += '.';
        out += decPart;
    }

    el.value = out;
}

