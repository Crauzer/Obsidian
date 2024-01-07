import { DevTool } from '@hookform/devtools';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { toast } from 'react-toastify';
import { z } from 'zod';

import { Button, Form, Kbd } from '../components';

type ComponentTestFormData = z.infer<typeof componentTestFormDataSchema>;
const componentTestFormDataSchema = z.object({
  testInput: z.string().min(5),
  testCheckbox: z.boolean(),
});

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
          <div className="flex flex-row items-center gap-2 bg-obsidian-600/50 p-2 text-gray-50">
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
      <div className="flex flex-col gap-2">
        <h1 className="text-xl text-gray-50">Toast:</h1>
        <div className="flex h-fit flex-row gap-2">
          <Button
            onClick={() => {
              toast('Hi', { type: 'default' });
            }}
          >
            Default
          </Button>
          <Button
            onClick={() => {
              toast('Hi', { type: 'info' });
            }}
          >
            Info
          </Button>
          <Button
            onClick={() => {
              toast('Hi', { type: 'success' });
            }}
          >
            Success
          </Button>
          <Button
            onClick={() => {
              toast('Hi', { type: 'error' });
            }}
          >
            Error
          </Button>
          <Button
            onClick={() => {
              toast('Hi', { type: 'warning' });
            }}
          >
            Warning
          </Button>
        </div>
      </div>
      <FormCard />
    </div>
  );
}

const FormCard = () => {
  const { handleSubmit, control } = useForm<ComponentTestFormData>({
    resolver: zodResolver(componentTestFormDataSchema),
  });

  const handleFormSubmit = (data: ComponentTestFormData) => {
    console.info(data);
  };

  return (
    <div className="flex flex-col gap-2">
      <h1 className="text-xl text-gray-50">Form:</h1>
      <div className="flex flex-row gap-2">
        <form className="flex flex-col gap-2" onSubmit={handleSubmit(handleFormSubmit)}>
          <Form.Input control={control} name="testInput" label="ffgff" />
          <Form.Checkbox control={control} name="testCheckbox">
            Test 123
          </Form.Checkbox>
          <Button type="submit">Submit</Button>
        </form>
        <DevTool control={control} />
      </div>
    </div>
  );
};
