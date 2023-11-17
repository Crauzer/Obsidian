import { ArrayPath, Control, FieldValues, useFieldArray } from 'react-hook-form';

type WadHashtableListProps<TFieldValues extends FieldValues> = {
  name: ArrayPath<TFieldValues>;
  control: Control<TFieldValues>;
};

export const WadHashtableList = <TFieldValues extends FieldValues>({
  name,
  control,
}: WadHashtableListProps<TFieldValues>) => {
  const {} = useFieldArray({ name, control });
};
