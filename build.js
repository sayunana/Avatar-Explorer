const fs = require("fs");
const path = require("path");

if (!fs.existsSync("Datas")) {
    fs.mkdirSync("Datas");
}

if (!fs.existsSync("Datas/AuthorImage")) {
    fs.mkdirSync("Datas/AuthorImage");
}

if (!fs.existsSync("Datas/Thumbnail")) {
    fs.mkdirSync("Datas/Thumbnail");
}

if (!fs.existsSync("Datas/Translate")) {
    fs.mkdirSync("Datas/Translate");
}

const TRANSLATE_FILES = "../../../Translate Files";
const translateFiles = fs.readdirSync(TRANSLATE_FILES);
const translateFilesLength = translateFiles.length;
console.log("Translate Files: " + translateFilesLength);

for (let i = 0; i < translateFilesLength; i++) {
    const file = translateFiles[i];
    const filePath = path.join(TRANSLATE_FILES, file);
    const destPath = path.join("Datas/Translate", file);
    fs.copyFileSync(filePath, destPath);
    console.log("Copying: " + file);
}

const ICON_FILES = "../../../Icons";
const iconFiles = fs.readdirSync(ICON_FILES);
const iconFilesLength = iconFiles.length;
console.log("Icon Files: " + iconFilesLength);

for (let i = 0; i < iconFilesLength; i++) {
    const file = iconFiles[i];
    const filePath = path.join(ICON_FILES, file);
    const destPath = path.join("Datas", file);
    fs.copyFileSync(filePath, destPath);
    console.log("Copying: " + file);
}

const FONT_FILES = "../../../Font Files";
const fontFiles = fs.readdirSync(FONT_FILES);
const fontFilesLength = fontFiles.length;
console.log("Font Files: " + fontFilesLength);

for (let i = 0; i < fontFilesLength; i++) {
    const file = fontFiles[i];
    const filePath = path.join(FONT_FILES, file);
    const destPath = path.join("Datas", file);
    fs.copyFileSync(filePath, destPath);
    console.log("Copying: " + file);
}

if (!fs.existsSync("Datas/ItemsData.json")) {
    fs.writeFileSync("Datas/ItemsData.json", "[]");
}

fs.rmSync("build.js");
console.log("Build Completed!");