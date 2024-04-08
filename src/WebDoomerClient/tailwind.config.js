/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ['./src/**/*.{html,ts}'],
    theme: {
        extend: {},
    },
    plugins: [require('tailwindcss-bg-patterns')],
    darkMode: 'class',
};
