import { Button, Kbd } from '../components';

export default function ComponentTest() {
  return (
    <div className="flex flex-col gap-2 p-6">
      <div className="flex flex-col gap-2">
        <h1 className="text-xl text-gray-50">Buttons:</h1>
        <div className="flex flex-row gap-2">
          <Button variant="default">Default</Button>
          <Button variant="filled">Filled</Button>
          <Button variant="ghost">Ghost</Button>
          <Button variant="light">Light</Button>
          <Button variant="outline">Outline</Button>
        </div>
      </div>
      <div className="flex flex-col gap-2">
        <h1 className="text-xl text-gray-50">Kbd:</h1>
        <div className="flex flex-row gap-2">
          <div className="flex flex-row items-center gap-2 p-2 bg-obsidian-600/50 text-gray-50">
            <span>
              <Kbd>Ctrl</Kbd> + <Kbd>F</Kbd>
            </span>
          </div>
          <div className="flex flex-row items-center gap-2 p-2 text-gray-50">
            <span>
              <Kbd>Ctrl</Kbd> + <Kbd>F</Kbd>
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
