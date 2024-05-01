import {
  Control,
  FieldPath,
  FieldValues,
  Path,
  RegisterOptions,
  useController,
} from 'react-hook-form';
import { FaFolder } from 'react-icons/fa';

import { ActionIcon, TextField, TextFieldProps } from '../../../components';
import { usePickDirectory } from '../../fs';

export type FormDirectoryInputProps<
  TFieldValues extends FieldValues,
  TFieldName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
> = {
  name: TFieldName;
  control: Control<TFieldValues>;
} & TextFieldProps;

export const FormDirectoryInput = <TFieldValues extends FieldValues>({
  name,
  control,
  className,
  ...props
}: FormDirectoryInputProps<TFieldValues>) => {
  const {
    field: { onBlur, onChange, value, ref },
    fieldState: { error },
  } = useController({ name, control });

  const pickDirectory = usePickDirectory();

  return (
    <TextField
      {...props}
      ref={ref}
      className="min-w-[500px]"
      value={value ?? ''}
      onBlur={onBlur}
      onChange={onChange}
      error={error ? error.message ?? true : false}
      left={
        <ActionIcon
          type="button"
          icon={FaFolder}
          iconClassName="shadow"
          variant="ghost"
          onClick={() => {
            pickDirectory.mutate(
              {},
              {
                onSuccess: ({ path }) => {
                  onChange(path);
                },
              },
            );
          }}
        />
      }
    />
  );
};
