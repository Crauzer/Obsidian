import { DevTool } from "@hookform/devtools";
import { zodResolver } from "@hookform/resolvers/zod";
import type React from "react";
import { useCallback, useEffect } from "react";
import { FormProvider, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { Form } from "../../../components";
import {
  createSettingsFromFormData,
  type SettingsFormData,
  settingsFormDataSchema,
  useUpdateSettings,
} from "..";
import { FormDirectoryInput } from "./FormDirectoryInput";

export type SettingsFormProps = {
  defaultValues: SettingsFormData;
};

export const SettingsForm: React.FC<SettingsFormProps> = ({
  defaultValues,
}) => {
  const [t] = useTranslation("settings");

  const { mutate: updateSettingsMutate } = useUpdateSettings();

  const formMethods = useForm<SettingsFormData>({
    reValidateMode: "onBlur",
    defaultValues,
    resolver: zodResolver(settingsFormDataSchema),
  });
  const { control, watch, handleSubmit } = formMethods;

  // update settings when any value changes
  const onSubmit = useCallback(
    (data: SettingsFormData) => {
      console.log("data", data);
      updateSettingsMutate({ settings: createSettingsFromFormData(data) });
    },
    [updateSettingsMutate],
  );
  useEffect(() => {
    const subscription = watch(() => handleSubmit(onSubmit)());

    return () => subscription.unsubscribe();
  }, [handleSubmit, onSubmit, watch]);

  return (
    <FormProvider {...formMethods}>
      <form className="flex w-full flex-col gap-8">
        <div className="flex flex-col gap-4">
          <Form.Checkbox control={control} name="openDirectoryAfterExtraction">
            {t("openDirectoryAfterExtraction")}
          </Form.Checkbox>
          <FormDirectoryInput
            control={control}
            name="defaultMountDirectory"
            label={t("defaultMountDirectory.label")}
          />
          <FormDirectoryInput
            control={control}
            name="defaultExtractionDirectory"
            label={t("defaultExtractionDirectory.label")}
          />
        </div>
        <DevTool control={control} />
      </form>
    </FormProvider>
  );
};
