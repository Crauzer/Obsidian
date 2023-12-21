import { DevTool } from '@hookform/devtools';
import { zodResolver } from '@hookform/resolvers/zod';
import React from 'react';
import { FormProvider, useForm } from 'react-hook-form';

import { SettingsFormData, settingsFormDataSchema } from '..';
import { Button } from '../../../components';

export type SettingsFormProps = {};

export const SettingsForm: React.FC<SettingsFormProps> = ({}) => {
  const formMethods = useForm<SettingsFormData>({
    resolver: zodResolver(settingsFormDataSchema),
  });
  const { control, handleSubmit } = formMethods;

  const handleFormSubmit = (data: SettingsFormData) => {
    console.info(data);
  };

  return (
    <FormProvider {...formMethods}>
      <form onSubmit={handleSubmit(handleFormSubmit)} className="flex w-full flex-col gap-2">
        <DevTool control={control} />
        <Button type="submit" className="ml-auto">
          Submit
        </Button>
      </form>
    </FormProvider>
  );
};
