import {
  type Control,
  type FieldValues,
  type Path,
  useController,
} from "react-hook-form";

import { Input, type InputProps } from "..";

export type FormInputProps<TFieldValues extends FieldValues> = {
  name: Path<TFieldValues>;
  control: Control<TFieldValues>;
} & InputProps;

export const FormInput = <TFieldValues extends FieldValues>({
  name,
  control,
  className,
  ...props
}: FormInputProps<TFieldValues>) => {
  const {
    field: { onBlur, onChange, value, ref },
    fieldState: { error },
  } = useController({ name, control });

  return (
    <Input
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
