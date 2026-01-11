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

    var raw = (el.value || '').toString();

    // 「入力中の末尾 '.'」は保持したい（例: 12. と打った直後）
    var keepTrailingDot = raw.endsWith('.');

    // 1) 先頭符号
    var sign = '';
    if (allowSign && raw.charAt(0) === '-') {
        sign = '-';
        raw = raw.slice(1);
    }

    // 2) 数字と小数点以外を除去（e/E/+/- も消す）
    var v = raw.replace(/[^\d.]/g, '');

    // 3) 小数点は最初の1個だけ残す
    var firstDot = v.indexOf('.');
    if (firstDot !== -1) {
        v = v.slice(0, firstDot + 1) + v.slice(firstDot + 1).replace(/\./g, '');
    }

    // 4) 先頭 '.' は '0.' に正規化（連打/誤入力耐性UP）
    if (v.startsWith('.')) {
        v = '0' + v;
        firstDot = v.indexOf('.');
    }

    // 5) 整数部/小数部に分割
    var parts = v.split('.');
    var intPart = parts[0] || '';
    var decPart = (parts.length > 1) ? (parts[1] || '') : '';

    // 6) 先頭ゼロ整理（0.xxx は残す）
    // 例: 00012 -> 12, 000.5 -> 0.5
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
            keepTrailingDot = false; // 小数不可なら末尾ドットも捨てる
        } else if (decPart.length > maxDec) {
            decPart = decPart.slice(0, maxDec);
        }
    }

    // 8) 再構成
    var out = sign + intPart;
    if (firstDot !== -1 || keepTrailingDot) {
        // 小数点は入力中も保持したい
        out += '.';
        out += decPart;
    }

    // 9) 値更新（変化がある時だけ）＋caretを末尾へ
    if (el.value !== out) {
        el.value = out;
        try {
            // type=number でも多くのブラウザで効く（効かなければ無視されるだけ）
            el.setSelectionRange(out.length, out.length);
        } catch (e) { }
    }
}
