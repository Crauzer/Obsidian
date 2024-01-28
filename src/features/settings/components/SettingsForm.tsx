import { DevTool } from '@hookform/devtools';
import { zodResolver } from '@hookform/resolvers/zod';
import React from 'react';
import { FormProvider, useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { FaFolder } from 'react-icons/fa';

import { SettingsFormData, settingsFormDataSchema } from '..';
import { ActionIcon, Button, Form, TextField } from '../../../components';
import { usePickDirectory } from '../../fs';

export type SettingsFormProps = {};

export const SettingsForm: React.FC<SettingsFormProps> = ({}) => {
  const [t] = useTranslation('settings');

  const formMethods = useForm<SettingsFormData>({
    resolver: zodResolver(settingsFormDataSchema),
  });
  const { control, handleSubmit, setValue: setFormValue } = formMethods;

  const pickDirectory = usePickDirectory();

  const handleFormSubmit = (data: SettingsFormData) => {
    console.info(data);
  };

  return (
    <FormProvider {...formMethods}>
      <form onSubmit={handleSubmit(handleFormSubmit)} className="flex w-full flex-col gap-2">
        <DevTool control={control} />
        <Form.TextField
          name="defaultMountDirectory"
          label={t('defaultMountDirectory.label')}
          control={control}
          className="min-w-[500px]"
          left={
            <ActionIcon
              icon={FaFolder}
              iconClassName="shadow"
              variant="ghost"
              onClick={() => {
                pickDirectory.mutate(
                  {},
                  {
                    onSuccess: ({ path }) => {
                      setFormValue('defaultMountDirectory', path, {
                        shouldTouch: true,
                        shouldDirty: true,
                      });
                    },
                  },
                );
              }}
            />
          }
        />
        <Button type="submit" className="ml-auto">
          Submit
        </Button>
      </form>
    </FormProvider>
  );
};
