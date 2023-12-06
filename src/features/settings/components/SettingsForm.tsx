import { DevTool } from '@hookform/devtools';
import { zodResolver } from '@hookform/resolvers/zod';
import React from 'react';
import {
  ArrayPath,
  Control,
  FieldValues,
  FormProvider,
  Path,
  useFieldArray,
  useForm,
} from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { FaPlus } from 'react-icons/fa';

import { SettingsFormData, settingsFormDataSchema, useSettings, useUpdateSettings } from '..';
import { ActionIcon, Button, Icon } from '../../../components';

export type SettingsFormProps = {};

export const SettingsForm: React.FC<SettingsFormProps> = ({}) => {
  const formMethods = useForm<SettingsFormData>({
    resolver: zodResolver(settingsFormDataSchema),
  });
  const { register, control, handleSubmit, formState } = formMethods;

  const handleFormSubmit = (data: SettingsFormData) => {
    console.info(data);
  };

  return (
    <FormProvider {...formMethods}>
      <form onSubmit={handleSubmit(handleFormSubmit)} className="flex w-full flex-col gap-2">
        <DevTool control={control} />
        <TestList />
        <Button type="submit" className="ml-auto">
          Submit
        </Button>
      </form>
    </FormProvider>
  );
};

type TestListProps<TFieldValues extends FieldValues> = {
  control: Control<TFieldValues>;
};

const TestList = () => {
  const [t] = useTranslation('settings');

  const settings = useSettings();
  const updateSettings = useUpdateSettings();

  if (settings.isLoading) {
    return (
      <div className="flex w-full flex-col gap-1">
        <div className="flex flex-row items-center">
          <p className="text-sm text-gray-50">{t('wadHashtableSources.title')}:</p>
          <div className="ml-auto h-10 w-10 rounded bg-gray-600"></div>
        </div>
        <div className="min-w-50 h-4 animate-pulse rounded bg-gray-600"></div>
        <div className="h-4 animate-pulse rounded bg-gray-600"></div>
        <div className="h-4 animate-pulse rounded bg-gray-700"></div>
        <div className="h-4 animate-pulse rounded bg-gray-800"></div>
      </div>
    );
  }

  if (settings.isSuccess) {
    return (
      <div className="flex flex-col gap-1">
        <div className="flex flex-row items-center">
          <p className="text-sm text-gray-50">{t('wadHashtableSources.title')}:</p>
          <ActionIcon
            className="ml-auto"
            size="sm"
            variant="outline"
            icon={FaPlus}
            onClick={() => {}}
          />
        </div>
        <div className="min-h-[6rem] rounded border border-gray-600 bg-gray-800"></div>
      </div>
    );
  }
};
