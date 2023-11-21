import { DevTool } from '@hookform/devtools';
import { zodResolver } from '@hookform/resolvers/zod';
import { FormProvider, useForm } from 'react-hook-form';

import { SettingsFormData, settingsFormDataSchema } from '..';
import { Button } from '../../../components';

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
    <>
      <FormProvider {...formMethods}>
        <form onSubmit={handleSubmit(handleFormSubmit)}>
          <DevTool control={control} />
          <Button type="submit">Submit</Button>
        </form>
      </FormProvider>
    </>
  );
};
