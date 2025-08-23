import {
  type Control,
  type FieldValues,
  type Path,
  useController,
} from "react-hook-form";

import { TextField, type TextFieldProps } from "..";

export type FormTextFieldProps<TFieldValues extends FieldValues> = {
  name: Path<TFieldValues>;
  control: Control<TFieldValues>;
} & TextFieldProps;

export const FormTextField = <TFieldValues extends FieldValues>({
  name,
  control,
  className,
  ...props
}: FormTextFieldProps<TFieldValues>) => {
  const {
    field: { onBlur, onChange, value, ref },
    fieldState: { error },
  } = useController({ name, control });

  return (
    <TextField
      {...props}
      ref={ref}
      className={className}
      value={value}
      onChange={onChange}
      onBlur={onBlur}
      error={error ? (error.message ?? true) : false}
    />
  );
};
