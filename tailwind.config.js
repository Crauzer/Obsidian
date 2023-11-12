/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    fontFamily: {
      'fira-sans': ['Fira Sans', 'sans-serif'],
      'fira-mono': ['Fira Mono', 'monospace'],
    },
    keyframes: {
      fadeIn: {
        from: { opacity: '0' },
        to: { opacity: '1' },
      },
      fadeOut: {
        from: { opacity: '1' },
        to: { opacity: '0' },
      },

      slideAndFadeInFromTop: {
        from: { opacity: '0', transform: 'translateY(-25px)' },
        to: { opacity: '1', transform: 'translateY(0)' },
      },
      slideAndFadeInFromRight: {
        from: { opacity: '0', transform: 'translateX(25px)' },
        to: { opacity: '1', transform: 'translateX(0)' },
      },
      slideAndFadeInFromBottom: {
        from: { opacity: '0', transform: 'translateY(25px)' },
        to: { opacity: '1', transform: 'translateY(0)' },
      },
      slideAndFadeInFromLeft: {
        from: { opacity: '0', transform: 'translateX(-25px)' },
        to: { opacity: '1', transform: 'translateX(0)' },
      },

      slideAndFadeOutFromTop: {
        from: { opacity: '1', transform: 'translateY(0)' },
        to: { opacity: '0', transform: 'translateY(-25px)' },
      },
      slideAndFadeOutFromRight: {
        from: { opacity: '1', transform: 'translateX(0)' },
        to: { opacity: '0', transform: 'translateX(25px)' },
      },
      slideAndFadeOutFromBottom: {
        from: { opacity: '1', transform: 'translateY(0)' },
        to: { opacity: '0', transform: 'translateY(25px)' },
      },
      slideAndFadeOutFromLeft: {
        from: { opacity: '1', transform: 'translateX(0)' },
        to: { opacity: '0', transform: 'translateX(-25px)' },
      },
    },
    animation: {
      fadeIn: 'fadeIn 150ms ease-in',
      fadeOut: 'fadeOut 150ms ease-in',

      slideAndFadeInFromTop:
        'slideAndFadeInFromTop 250ms cubic-bezier(0.250, 0.460, 0.450, 0.940) both',
      slideAndFadeInFromRight:
        'slideAndFadeInFromRight 250ms cubic-bezier(0.250, 0.460, 0.450, 0.940) both',
      slideAndFadeInFromBottom:
        'slideAndFadeInFromBottom 250ms cubic-bezier(0.250, 0.460, 0.450, 0.940) both',
      slideAndFadeInFromLeft:
        'slideAndFadeInFromLeft 250ms cubic-bezier(0.250, 0.460, 0.450, 0.940) both',

      slideAndFadeOutFromTop:
        'slideAndFadeOutFromTop 250ms cubic-bezier(0.250, 0.460, 0.450, 0.940) both',
      slideAndFadeOutFromRight:
        'slideAndFadeOutFromRight 250ms cubic-bezier(0.250, 0.460, 0.450, 0.940) both',
      slideAndFadeOutFromBottom:
        'slideAndFadeOutFromBottom 250ms cubic-bezier(0.250, 0.460, 0.450, 0.940) both',
      slideAndFadeOutFromLeft:
        'slideAndFadeOutFromLeft 250ms cubic-bezier(0.250, 0.460, 0.450, 0.940) both',
    },
    extend: {
      colors: {
        obsidian: {
          50: '#fff2f1',
          100: '#ffe5e4',
          200: '#fdcfce',
          300: '#fca5a5',
          400: '#fa7275',
          500: '#f24149',
          600: '#df1f32',
          700: '#bc1429',
          800: '#9d1429',
          900: '#851429',
          950: '#4b0611',
        },
        gray: {
          50: '#f9fafb',
          100: '#f3f4f6',
          200: '#e5e7eb',
          300: '#d1d5db',
          400: '#9ca3af',
          500: '#6b7280',
          600: '#4b5563',
          700: '#374151',
          800: '#1f2937',
          900: '#111827',
          950: '#030712',
        },
      },
    },
  },
  plugins: [],
};
