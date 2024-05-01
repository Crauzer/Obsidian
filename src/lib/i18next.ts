import i18next from 'i18next';
import { initReactI18next } from 'react-i18next';

import { en } from '../i18n';
import { env } from '../utils';

export const defaultNamespace = 'common' as const;
export const resources = { en } as const;

i18next
  .use(initReactI18next)
  .init({ debug: env.DEV, lng: 'en', resources, defaultNS: defaultNamespace });

export default i18next;
