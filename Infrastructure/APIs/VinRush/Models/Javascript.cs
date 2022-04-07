using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.APIs.VinRush.Models
{
    internal static class Javascript
    {
        internal static string Js = @"
'use strict';
var _0x213c = ['is-valid', 'dec_btn', 'innerHTML', 'location', 'href', 'https://www.vinrush.com/', '/decode-check/', 'toUpperCase', 'test', 'ERROR: VIN number must contain 17 characters.', 'length', 'charCodeAt', 'substr', 'replace', 'fromCharCode', 'trim', 'getElementById', 'vin_number', 'value', 'classList', 'add', 'remove'];
(function(data, i) {
 var validateGroupedContexts = function fn(selected_image) {
   for (; --selected_image;) {
     data['push'](data['shift']());
   }
 };
 validateGroupedContexts(++i);
})(_0x213c, 318);
var _0x54b8 = function childAdd(index, child_path) {
 index = index - 0;
 var source = _0x213c[index];
 return source;
};
var skew = function init(obj) {
 function error(x, n) {
   return x << n | x >>> 32 - n;
 }
 function $(str, a) {
   var b;
   var c;
   var tmp30;
   var tmp24;
   var tmp35;
   tmp30 = str & 2147483648;
   tmp24 = a & 2147483648;
   b = str & 1073741824;
   c = a & 1073741824;
   tmp35 = (str & 1073741823) + (a & 1073741823);
   if (b & c) {
     return tmp35 ^ 2147483648 ^ tmp30 ^ tmp24;
   }
   if (b | c) {
     if (tmp35 & 1073741824) {
       return tmp35 ^ 3221225472 ^ tmp30 ^ tmp24;
     } else {
       return tmp35 ^ 1073741824 ^ tmp30 ^ tmp24;
     }
   } else {
     return tmp35 ^ tmp30 ^ tmp24;
   }
 }
 function expect(n, t, a) {
   return n & t | ~n & a;
 }
 function load(n, t, a) {
   return n & a | t & ~a;
 }
 function get(searchTerm, callback, opt_key) {
   return searchTerm ^ callback ^ opt_key;
 }
 function size(c, d, a) {
   return d ^ (c | ~a);
 }
 function f(value, input, id, type, data, e, parent) {
   value = $(value, $($(expect(input, id, type), data), parent));
   return $(error(value, e), input);
 }
 function fn(a, item, data, source, padStr, type, parent) {
   a = $(a, $($(load(item, data, source), padStr), parent));
   return $(error(a, type), item);
 }
 function create(value, item, val, name, appQuitHandler, id, parent) {
   value = $(value, $($(get(item, val, name), appQuitHandler), parent));
   return $(error(value, id), item);
 }
 function debug(str, obj, i, data, params, name, vars) {
   str = $(str, $($(size(obj, i, data), params), vars));
   return $(error(str, name), obj);
 }
 function ConvertToWordArray(string) {
   var lWordCount;
   var lMessageLength = string[_0x54b8('0x0')];
   var lNumberOfWords_temp1 = lMessageLength + 8;
   var _0x547e15 = (lNumberOfWords_temp1 - lNumberOfWords_temp1 % 64) / 64;
   var lNumberOfWords = (_0x547e15 + 1) * 16;
   var lWordArray = Array(lNumberOfWords - 1);
   var lBytePosition = 0;
   var lByteCount = 0;
   for (; lByteCount < lMessageLength;) {
     lWordCount = (lByteCount - lByteCount % 4) / 4;
     lBytePosition = lByteCount % 4 * 8;
     lWordArray[lWordCount] = lWordArray[lWordCount] | string[_0x54b8('0x1')](lByteCount) << lBytePosition;
     lByteCount++;
   }
   lWordCount = (lByteCount - lByteCount % 4) / 4;
   lBytePosition = lByteCount % 4 * 8;
   lWordArray[lWordCount] = lWordArray[lWordCount] | 128 << lBytePosition;
   lWordArray[lNumberOfWords - 2] = lMessageLength << 3;
   lWordArray[lNumberOfWords - 1] = lMessageLength >>> 29;
   return lWordArray;
 }
 function extend(data) {
   var destination = '';
   var th_field = '';
   var intval;
   var i;
   i = 0;
   for (; i <= 3; i++) {
     intval = data >>> i * 8 & 255;
     th_field = '0' + intval['toString'](16);
     destination = destination + th_field[_0x54b8('0x2')](th_field[_0x54b8('0x0')] - 2, 2);
   }
   return destination;
 }
 function testcase(fn) {
   fn = fn[_0x54b8('0x3')](/\r\n/g, '\n');
   var result = '';
   var req = 0;
   for (; req < fn[_0x54b8('0x0')]; req++) {
     var data = fn['charCodeAt'](req);
     if (data < 128) {
       result = result + String[_0x54b8('0x4')](data);
     } else {
       if (data > 127 && data < 2048) {
         result = result + String['fromCharCode'](data >> 6 | 192);
         result = result + String[_0x54b8('0x4')](data & 63 | 128);
       } else {
         result = result + String[_0x54b8('0x4')](data >> 12 | 224);
         result = result + String['fromCharCode'](data >> 6 & 63 | 128);
         result = result + String[_0x54b8('0x4')](data & 63 | 128);
       }
     }
   }
   return result;
 }
 var args = Array();
 var i;
 var string;
 var viewChart;
 var div;
 var rotationQuaternion;
 var name;
 var data;
 var result;
 var value;
 var w = 7;
 var j = 12;
 var cplext = 17;
 var end = 22;
 var type = 5;
 var t = 9;
 var key = 14;
 var upsert = 20;
 var ns = 4;
 var prev = 11;
 var B = 16;
 var views = 23;
 var m = 6;
 var pathname = 10;
 var line = 15;
 var count = 21;
 obj = testcase(obj);
 args = ConvertToWordArray(obj);
 name = 1732584193;
 data = 4023233417;
 result = 2562383102;
 value = 271733878;
 i = 0;
 for (; i < args[_0x54b8('0x0')]; i = i + 16) {
   string = name;
   viewChart = data;
   div = result;
   rotationQuaternion = value;
   name = f(name, data, result, value, args[i + 0], w, 3614090360);
   value = f(value, name, data, result, args[i + 1], j, 3905402710);
   result = f(result, value, name, data, args[i + 2], cplext, 606105819);
   data = f(data, result, value, name, args[i + 3], end, 3250441966);
   name = f(name, data, result, value, args[i + 4], w, 4118548399);
   value = f(value, name, data, result, args[i + 5], j, 1200080426);
   result = f(result, value, name, data, args[i + 6], cplext, 2821735955);
   data = f(data, result, value, name, args[i + 7], end, 4249261313);
   name = f(name, data, result, value, args[i + 8], w, 1770035416);
   value = f(value, name, data, result, args[i + 9], j, 2336552879);
   result = f(result, value, name, data, args[i + 10], cplext, 4294925233);
   data = f(data, result, value, name, args[i + 11], end, 2304563134);
   name = f(name, data, result, value, args[i + 12], w, 1804603682);
   value = f(value, name, data, result, args[i + 13], j, 4254626195);
   result = f(result, value, name, data, args[i + 14], cplext, 2792965006);
   data = f(data, result, value, name, args[i + 15], end, 1236535329);
   name = fn(name, data, result, value, args[i + 1], type, 4129170786);
   value = fn(value, name, data, result, args[i + 6], t, 3225465664);
   result = fn(result, value, name, data, args[i + 11], key, 643717713);
   data = fn(data, result, value, name, args[i + 0], upsert, 3921069994);
   name = fn(name, data, result, value, args[i + 5], type, 3593408605);
   value = fn(value, name, data, result, args[i + 10], t, 38016083);
   result = fn(result, value, name, data, args[i + 15], key, 3634488961);
   data = fn(data, result, value, name, args[i + 4], upsert, 3889429448);
   name = fn(name, data, result, value, args[i + 9], type, 568446438);
   value = fn(value, name, data, result, args[i + 14], t, 3275163606);
   result = fn(result, value, name, data, args[i + 3], key, 4107603335);
   data = fn(data, result, value, name, args[i + 8], upsert, 1163531501);
   name = fn(name, data, result, value, args[i + 13], type, 2850285829);
   value = fn(value, name, data, result, args[i + 2], t, 4243563512);
   result = fn(result, value, name, data, args[i + 7], key, 1735328473);
   data = fn(data, result, value, name, args[i + 12], upsert, 2368359562);
   name = create(name, data, result, value, args[i + 5], ns, 4294588738);
   value = create(value, name, data, result, args[i + 8], prev, 2272392833);
   result = create(result, value, name, data, args[i + 11], B, 1839030562);
   data = create(data, result, value, name, args[i + 14], views, 4259657740);
   name = create(name, data, result, value, args[i + 1], ns, 2763975236);
   value = create(value, name, data, result, args[i + 4], prev, 1272893353);
   result = create(result, value, name, data, args[i + 7], B, 4139469664);
   data = create(data, result, value, name, args[i + 10], views, 3200236656);
   name = create(name, data, result, value, args[i + 13], ns, 681279174);
   value = create(value, name, data, result, args[i + 0], prev, 3936430074);
   result = create(result, value, name, data, args[i + 3], B, 3572445317);
   data = create(data, result, value, name, args[i + 6], views, 76029189);
   name = create(name, data, result, value, args[i + 9], ns, 3654602809);
   value = create(value, name, data, result, args[i + 12], prev, 3873151461);
   result = create(result, value, name, data, args[i + 15], B, 530742520);
   data = create(data, result, value, name, args[i + 2], views, 3299628645);
   name = debug(name, data, result, value, args[i + 0], m, 4096336452);
   value = debug(value, name, data, result, args[i + 7], pathname, 1126891415);
   result = debug(result, value, name, data, args[i + 14], line, 2878612391);
   data = debug(data, result, value, name, args[i + 5], count, 4237533241);
   name = debug(name, data, result, value, args[i + 12], m, 1700485571);
   value = debug(value, name, data, result, args[i + 3], pathname, 2399980690);
   result = debug(result, value, name, data, args[i + 10], line, 4293915773);
   data = debug(data, result, value, name, args[i + 1], count, 2240044497);
   name = debug(name, data, result, value, args[i + 8], m, 1873313359);
   value = debug(value, name, data, result, args[i + 15], pathname, 4264355552);
   result = debug(result, value, name, data, args[i + 6], line, 2734768916);
   data = debug(data, result, value, name, args[i + 13], count, 1309151649);
   name = debug(name, data, result, value, args[i + 4], m, 4149444226);
   value = debug(value, name, data, result, args[i + 11], pathname, 3174756917);
   result = debug(result, value, name, data, args[i + 2], line, 718787259);
   data = debug(data, result, value, name, args[i + 9], count, 3951481745);
   name = $(name, string);
   data = $(data, viewChart);
   result = $(result, div);
   value = $(value, rotationQuaternion);
 }
 var B58 = extend(name) + extend(data) + extend(result) + extend(value);
 return B58['toLowerCase']();
};


//skew('WBAAL11010JN15855test').substring(0,5);

function jsAdd(a, b) { return a + b; }
";
    }
}
