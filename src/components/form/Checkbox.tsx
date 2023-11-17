import { CheckedState } from '@radix-ui/react-checkbox';
import { Control, FieldValues, Path, useController } from 'react-hook-form';

import { Checkbox, CheckboxProps } from '..';

export type FormCheckboxProps<TFieldValues extends FieldValues> = {
  name: Path<TFieldValues>;
  control: Control<TFieldValues>;
} & CheckboxProps;

export const FormCheckbox = <TFieldValues extends FieldValues>({
  name,
  control,
  className,
  ...props
}: FormCheckboxProps<TFieldValues>) => {
  const {
    field: { onBlur, onChange, value, ref },
  } = useController({ name, control });

  const handleCheckedChange = (checked: CheckedState) => {
    onChange(checked === 'indeterminate' ? undefined : checked);
  };

  return (
    <Checkbox
      {...props}
      ref={ref}
      className={className}
      checked={value}
      onCheckedChange={handleCheckedChange}
      onBlur={onBlur}
    />
  );
};
