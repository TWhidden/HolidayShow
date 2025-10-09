import { IDisposable } from ".";

export class DisposableBase implements IDisposable {
    protected readonly _disposers: (() => void)[] = [];
 
    dispose() {
        this._disposers.forEach((disposer) => {
            disposer();
        });
 
        this._disposers.length = 0;
    }
}