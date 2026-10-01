import localFont from "next/font/local";

export const inter = localFont({
  variable: "--font-inter",
  src: [
    { path: "./InterVariable.ttf" },
    { path: "./InterItalicVariable.ttf", style: "italic" },
  ],
});

export const notoSansMono = localFont({
  variable: "--font-noto-sans-mono",
  src: "./NotoSansMono.ttf",
  style: "mono",
});

export const fonts = { inter, notoSansMono };

export default fonts;
