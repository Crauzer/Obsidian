import { t } from 'i18next';
import { match } from 'ts-pattern';
import { z } from 'zod';

const windowsPathRegex =
  /^(?<ParentPath>(?:[a-zA-Z]:|\\\\[\w\s.]+\\[\w\s.$]+)\\(?:[\w\s.]+\\)*)(?<BaseName>[\w\s.]*?)$/gm;

export const pathStringSchema = z
  .string({
    errorMap: (issue, ctx) =>
      match(issue.code)
        .with('invalid_string', () => ({ message: t('validation:mustBeAPath') }))
        .otherwise(() => ({
          message: ctx.defaultError,
        })),
  })
  .regex(windowsPathRegex);
