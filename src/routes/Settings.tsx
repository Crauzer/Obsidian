import { SettingsForm } from '../features/settings';

export default function Settings() {
  return (
    <div className="flex w-full items-center justify-center">
      <div className="w-[1280px] max-w-[1280px] px-4">
        <SettingsForm />
      </div>
    </div>
  );
}
