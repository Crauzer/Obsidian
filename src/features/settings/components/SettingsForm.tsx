import { DevTool } from '@hookform/devtools';
import { zodResolver } from '@hookform/resolvers/zod';
import React from 'react';
import { FormProvider, useForm } from 'react-hook-form';
import { FaFolder } from 'react-icons/fa';

import { SettingsFormData, settingsFormDataSchema } from '..';
import { ActionIcon, Button, TextField } from '../../../components';

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
        <TextField left={<ActionIcon icon={FaFolder} variant="ghost" />} />
        <Button type="submit" className="ml-auto">
          Submit
        </Button>
      </form>
    </FormProvider>
  );
};
