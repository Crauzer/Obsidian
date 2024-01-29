import { DevTool } from '@hookform/devtools';
import { zodResolver } from '@hookform/resolvers/zod';
import React from 'react';
import { FormProvider, SubmitErrorHandler, useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { toast } from 'react-toastify';

import {
  SettingsFormData,
  createSettingsFromFormData,
  settingsFormDataSchema,
  useSettings,
  useUpdateSettings,
} from '..';
import { Button, Toast } from '../../../components';
import { toastAutoClose } from '../../../utils/toast';
import { FormDirectoryInput } from './FormDirectoryInput';

export type SettingsFormProps = {};

export const SettingsForm: React.FC<SettingsFormProps> = ({}) => {
  const [t] = useTranslation('settings');

  const settings = useSettings();
  const updateSettings = useUpdateSettings();

  const formMethods = useForm<SettingsFormData>({
    values: settings.data,
    resolver: zodResolver(settingsFormDataSchema),
  });
  const { control, handleSubmit } = formMethods;

  const handleFormSubmit = (data: SettingsFormData) => {
    updateSettings.mutate(
      { settings: createSettingsFromFormData(data) },
      {
        onSuccess: () => {
          toast.success(<Toast.Success message={t('submit.success')} />, {
            autoClose: toastAutoClose.veryShort,
          });
        },
      },
    );
  };

  return (
    <FormProvider {...formMethods}>
      <form onSubmit={handleSubmit(handleFormSubmit)} className="flex w-full flex-col gap-8">
        <FormDirectoryInput
          control={control}
          name="defaultMountDirectory"
          label={t('defaultMountDirectory.label')}
        />
        <DevTool control={control} />

        <Button type="submit" variant="filled" className="ml-auto">
          Submit
        </Button>
      </form>
    </FormProvider>
  );
};
