export function blendColors(hex1: string, hex2: string, ratio: number): string {
    // Blend the hue, saturation, and value of the two colors
    let rgb1 = hexToRgb(hex1);
    let rgb2 = hexToRgb(hex2);

    let hsv1 = rgbToHsv(rgb1);
    let hsv2 = rgbToHsv(rgb2);

    let h = (hsv1[0] * (1 - ratio) + hsv2[0] * ratio) % 360;
    let s = hsv1[1] * (1 - ratio) + hsv2[1] * ratio;
    let v = hsv1[2] * (1 - ratio) + hsv2[2] * ratio;

    let rgb = hsvToRgb([h, s, v]);
    let hex = rgbToHex(rgb);

    return hex;
}

export function hexToRgb(hex: string): [number, number, number] {
    let r = parseInt(hex.substring(1, 3), 16);
    let g = parseInt(hex.substring(3, 5), 16);
    let b = parseInt(hex.substring(5, 7), 16);

    return [r, g, b];
}

export function rgbToHsv(rgb: [number, number, number]): [number, number, number] {
    let r = rgb[0] / 255;
    let g = rgb[1] / 255;
    let b = rgb[2] / 255;

    let cmax = Math.max(r, g, b);
    let cmin = Math.min(r, g, b);
    let delta = cmax - cmin;

    let h = 0;
    let s = 0;
    let v = 0;

    if (delta == 0) {
        h = 0;
    } else if (cmax == r) {
        h = ((g - b) / delta) % 6;
    } else if (cmax == g) {
        h = (b - r) / delta + 2;
    } else {
        h = (r - g) / delta + 4;
    }

    h = Math.round(h * 60);

    if (h < 0) {
        h += 360;
    }

    v = cmax;
    s = delta == 0 ? 0 : delta / cmax;

    s = +(s * 100).toFixed(1);
    v = +(v * 100).toFixed(1);

    return [h, s, v];
}

export function hsvToRgb(hsv: [number, number, number]): [number, number, number] {
    let h = hsv[0];
    let s = hsv[1] / 100;
    let v = hsv[2] / 100;

    let c = v * s;
    let x = c * (1 - Math.abs(((h / 60) % 2) - 1));
    let m = v - c;

    let r = 0;
    let g = 0;
    let b = 0;

    if (h >= 0 && h < 60) {
        r = c;
        g = x;
        b = 0;
    } else if (h >= 60 && h < 120) {
        r = x;
        g = c;
        b = 0;
    } else if (h >= 120 && h < 180) {
        r = 0;
        g = c;
        b = x;
    } else if (h >= 180 && h < 240) {
        r = 0;
        g = x;
        b = c;
    } else if (h >= 240 && h < 300) {
        r = x;
        g = 0;
        b = c;
    } else {
        r = c;
        g = 0;
        b = x;
    }

    r = Math.round((r + m) * 255);
    g = Math.round((g + m) * 255);
    b = Math.round((b + m) * 255);

    return [r, g, b];
}

export function rgbToHex(rgb: [number, number, number]): string {
    let r = rgb[0].toString(16);
    let g = rgb[1].toString(16);
    let b = rgb[2].toString(16);

    if (r.length == 1) {
        r = "0" + r;
    }

    if (g.length == 1) {
        g = "0" + g;
    }

    if (b.length == 1) {
        b = "0" + b;
    }

    return "#" + r + g + b;
}
