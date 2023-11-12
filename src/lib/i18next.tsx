import i18next from 'i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import { initReactI18next } from 'react-i18next';

import { en } from '../i18n';
import { env } from '../utils';

export const defaultNamespace = 'common';
export const resources = { en } as const;

i18next.use(LanguageDetector).use(initReactI18next).init({ debug: env.DEV, resources });

export default i18next;
